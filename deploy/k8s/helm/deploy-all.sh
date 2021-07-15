#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode
set -euo pipefail

usage()
{
  cat <<END
deploy.sh: deploys the $app_name application to a Kubernetes cluster using Helm.
Parameters:
  --aks-name <AKS cluster name>
    The name of the AKS cluster. Required when the registry (using the -r parameter) is set to "aks".
  --aks-rg <AKS resource group>
    The resource group for the AKS cluster. Required when the registry (using the -r parameter) is set to "aks".
  -b | --build-solution
    Force a solution build before deployment (default: false).
  -d | --dns <dns or ip address> | --dns aks
    Specifies the external DNS/ IP address of the Kubernetes cluster.
    If 'aks' is set as value, the DNS value is retrieved from the AKS. --aks-name and --aks-rg are needed.
    When --use-local-k8s is specified the external DNS is automatically set to localhost.
  -h | --help
    Displays this help text and exits the script.
  --image-build
    Build images (default is to not build all images).
  --image-push
    Upload images to the container registry (default is not pushing to the custom registry)
  -n | --app-name <the name of the app>
    Specifies the name of the application (default: eshop).
  --namespace <namespace name>
    Specifies the namespace name to deploy the app. If it doesn't exists it will be created (default: eshop).
  -p | --docker-password <docker password>
    The Docker password used to logon to the custom registry, supplied using the -r parameter.
  -r | --registry <container registry>
    Specifies the container registry to use (required), e.g. myregistry.azurecr.io.
  --skip-clean
    Do not clean the Kubernetes cluster (default is to clean the cluster).
  --skip-infrastructure
    Do not deploy infrastructure resources (like sql-data, no-sql or redis).
    This is useful for production environments where infrastructure is hosted outside the Kubernetes cluster.
  -t | --tag <docker image tag>
    The tag used for the newly created docker images. Default: latest.
  -u | --docker-username <docker username>
    The Docker username used to logon to the custom registry, supplied using the -r parameter.
  --use-local-k8s
    Deploy to a locally installed Kubernetes (default: false).

It is assumed that the Kubernetes cluster has been granted access to the container registry.
If using AKS and ACR see link for more info: 
https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-aks

WARNING! THE SCRIPT WILL COMPLETELY DESTROY ALL DEPLOYMENTS AND SERVICES VISIBLE
FROM THE CURRENT CONFIGURATION CONTEXT AND NAMESPACE.
It is recommended that you check your selected namespace, 'eshop' by default, is already in use.
Every deployment and service done in the namespace will be deleted.
For more information see https://kubernetes.io/docs/tasks/administer-cluster/namespaces/

END
}

app_name='eshop'
aks_name=''
aks_rg=''
build_images=''
clean='yes'
build_solution=''
container_registry=''
docker_password=''
docker_username=''
dns=''
image_tag='latest'
push_images=''
skip_infrastructure=''
use_local_k8s=''
namespace='eshop'

while [[ $# -gt 0 ]]; do
  case "$1" in
    --aks-name )
      aks_name="$2"; shift 2;;
    --aks-rg )
      aks_rg="$2"; shift 2;;
    -b | --build-solution )
      build_solution='yes'; shift ;;
    -d | --dns )
      dns="$2"; shift 2;;
    -h | --help )
      usage; exit 1 ;;
    -n | --app-name )
      app_name="$2"; shift 2;;
    -p | --docker-password )
      docker_password="$2"; shift 2;;
    -r | --registry )
      container_registry="$2"; shift 2;;
    --skip-clean )
      clean=''; shift ;;
    --image-build )
      build_images='yes'; shift ;;
    --image-push )
      push_images='yes'; shift ;;
    --skip-infrastructure )
      skip_infrastructure='yes'; shift ;;
    -t | --tag )
      image_tag="$2"; shift 2;;  
    -u | --docker-username )
      docker_username="$2"; shift 2;;
    --use-local-k8s )
      use_local_k8s='yes'; shift ;;
    --namespace )
      namespace="$2"; shift 2;;
    *)
      echo "Unknown option $1"
      usage; exit 2 ;;
  esac
done

if [[ $build_solution ]]; then
  echo "#################### Building $app_name solution ####################"
  dotnet publish -o obj/Docker/publish ../../eShopOnContainers-ServicesAndWebApps.sln
fi

export TAG=$image_tag

if [[ $build_images ]]; then
  echo "#################### Building the $app_name Docker images ####################"
  docker-compose -p ../.. -f ../../docker-compose.yml build

  # Remove temporary images
  docker rmi $(docker images -qf "dangling=true") 
fi

use_custom_registry=''

if [[ -n $container_registry ]]; then 
  echo "################ Log into custom registry $container_registry ##################"
  use_custom_registry='yes'
  if [[ -z $docker_username ]] || [[ -z $docker_password ]]; then
    echo "Error: Must use -u (--docker-username) AND -p (--docker-password) if specifying custom registry"
    exit 1
  fi
  docker login -u $docker_username -p $docker_password $container_registry
fi

if [[ $push_images ]]; then
  echo "#################### Pushing images to the container registry ####################"
  services=(basket.api catalog.api identity.api ordering.api payment.api webmvc webspa webstatus)

  if [[ -z "$(docker image ls -q --filter=reference=eshop/$service:$image_tag)" ]]; then
    image_tag=linux-$image_tag
  fi

  for service in "${services[@]}"
  do
    echo "Pushing image for service $service..."
    docker tag "eshop/$service:$image_tag" "$container_registry/$service:$image_tag"
    docker push "$container_registry/$service:$image_tag"
  done
fi

ingress_values_file="ingress_values.yaml"

if [[ $use_local_k8s ]]; then
  ingress_values_file="ingress_values_dockerk8s.yaml"
  dns="host.docker.internal"
fi

if [[ $dns == "aks" ]]; then
  echo "#################### Begin AKS discovery based on the --dns aks setting. ####################"
  if [[ -z $aks_name ]] || [[ -z $aks_rg ]]; then
    echo "Error: When using -dns aks, MUST set -aksName and -aksRg too."
    echo ''
    usage
    exit 1
  fi

  echo "Getting AKS cluster $aks_name  AKS (in resource group $aks_rg)"
  # JMESPath queries are case sensitive and httpapplicationrouting can be lowercase sometimes
  jmespath_dnsqueries=(\
    addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName \
    addonProfiles.httpapplicationrouting.config.HTTPApplicationRoutingZoneName \
  )
  for q in "${jmespath_dnsqueries[@]}"
  do
    dns="$(az aks show -n $aks_name -g $aks_rg --query $q -o tsv)"
    if [[ -n $dns ]]; then break; fi
  done
  if [[ -z $dns ]]; then
    echo "Error: when getting DNS of AKS $aks_name (in resource group $aks_rg). Please ensure AKS has httpRouting enabled AND Azure CLI is logged in and is of version 2.0.37 or higher."
    exit 1
  fi
  echo "DNS base found is $dns. Will use $aks_name.$dns for the app!"
  dns="$aks_name.$dns"
fi

# Initialization & check commands
if [[ -z $dns ]]; then
  echo "No DNS specified. Ingress resources will be bound to public IP."
fi

if [[ $clean ]]; then
  echo "Cleaning previous helm releases..."
  if [[ -z $(helm ls -q --namespace $namespace) ]]; then
    echo "No previous releases found"
  else
    helm uninstall $(helm ls -q --namespace $namespace) --namespace $namespace
    echo "Previous releases deleted"
    waitsecs=10; while [ $waitsecs -gt 0 ]; do echo -ne "$waitsecs\033[0K\r"; sleep 1; : $((waitsecs--)); done
  fi
fi

echo "#################### Begin $app_name installation using Helm ####################"
infras=(sql-data nosql-data rabbitmq keystore-data basket-data)
charts=(eshop-common basket-api catalog-api identity-api mobileshoppingagg ordering-api ordering-backgroundtasks ordering-signalrhub payment-api webmvc webshoppingagg webspa webstatus webhooks-api webhooks-web)
apis=(apigwms apigwws)

if [[ !$skip_infrastructure ]]; then
  for infra in "${infras[@]}"
  do
    echo "Installing infrastructure: $infra"
    helm install "$app_name-$infra" --namespace $namespace --set "ingress.hosts={$dns}" --values app.yaml --values inf.yaml --values $ingress_values_file --set app.name=$app_name --set inf.k8s.dns=$dns $infra     
  done  
fi

for api in "${apis[@]}"
do
  echo "Installing: $api"
  if [[ $use_custom_registry ]]; then 
    helm install "$app_name-$api" --namespace $namespace --set "ingress.hosts={$dns}" --set inf.registry.server=$container_registry --set inf.registry.login=$docker_username --set inf.registry.pwd=$docker_password --set inf.registry.secretName=eshop-docker-scret --values app.yaml --values inf.yaml --values $ingress_values_file --set app.name=$app_name --set inf.k8s.dns=$dns --set image.tag=$image_tag --set image.pullPolicy=Always $api 
  else
    helm install "$app_name-$api" --namespace $namespace --set "ingress.hosts={$dns}" --values app.yaml --values inf.yaml --values $ingress_values_file --set app.name=$app_name --set inf.k8s.dns=$dns $api 
  fi
done

for chart in "${charts[@]}"
do
  echo "Installing: $chart"
  if [[ $use_custom_registry ]]; then 
    helm install "$app_name-$chart" --namespace $namespace --set "ingress.hosts={$dns}" --set inf.registry.server=$container_registry --set inf.registry.login=$docker_username --set inf.registry.pwd=$docker_password --set inf.registry.secretName=eshop-docker-scret --values app.yaml --values inf.yaml --values $ingress_values_file --set app.name=$app_name --set inf.k8s.dns=$dns --set image.tag=$image_tag --set image.pullPolicy=Always $chart 
  elif [[ $chart != "eshop-common" ]]; then  # eshop-common is ignored when no secret must be deployed
    helm install "$app_name-$chart" --namespace $namespace --set "ingress.hosts={$dns}" --values app.yaml --values inf.yaml --values $ingress_values_file --set app.name=$app_name --set inf.k8s.dns=$dns --set image.tag=$image_tag --set image.pullPolicy=Always $chart 
  fi
done

echo "FINISHED: Helm charts installed."
