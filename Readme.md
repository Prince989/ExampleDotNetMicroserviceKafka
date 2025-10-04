## Project Setup

### 1. Setup Minikube

```powershell
minikube start
minikube docker-env | Invoke-Expression
```

### 2. Build Docker images

```powershell
# AuthService
cd .\AuthService\
docker build -t auth-service:dev . -f .\src\AuthService.Api\Dockerfile

# ProductService
cd .\ProductService\
docker build -t product-service:dev . -f .\src\ProductService.Api\Dockerfile

# SearchService
cd .\SearchService\
docker build -t search-service:dev . -f .\src\SearchService.Api\Dockerfile

# ApiGateway
cd .\ApiGateway\
docker build -t api-gateway:dev .
```

### 3. Loading images to minikube

```powershell
minikube image load auth-service:dev
minikube image load product-service:dev
minikube image load search-service:dev
minikube image load api-gateway:dev
```

### 4. Applying services and configs to the Kubernetes
```powershell
kubectl apply -f ./k8s/Market-k8s.yaml
```

### 5. Installing Istio requirements

```powershell
istioctl install --set profile=demo -y
```
### 6. Checking pods

```powershell
kubectl get pods -n marketplace
kubectl get pods -n istio-system
```

### Access to the services
For getting and access to the Ingress Gateway:

```powershell
minikube service istio-ingressgateway -n istio-system --url
```
The Second address is the api gateway address

```powershell
http://<second-address>/swagger/index.html
```

## In the case of reseting minikube environment:

```powershell
minikube delete
```


## Istio Configurations

### Timeout and retry
just for the example I put a virtual service for the auth service and in this configuration block you can set the timeout and retry settings

```yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: auth-service-vs
  namespace: marketplace
spec:
  hosts:
    - auth-service-svc.marketplace.svc.cluster.local
  http:
    - route:
        - destination:
            host: auth-service-svc.marketplace.svc.cluster.local
            port:
              number: 8000
      timeout: 10s
      retries:
        attempts: 2
        perTryTimeout: 10s
        retryOn: "connect-failure,refused-stream"
```

### mtls and security

These configurations are global that means it applies to every services that exist in the marketplace namespace
You can change the mtls setting in peer authentication config

you can select the mode : STRICT or PERMISSIVE or UNSET or DISABLE

```yaml
apiVersion: security.istio.io/v1
kind: PeerAuthentication
metadata:
  name: default
  namespace: marketplace
spec:
  mtls:
    mode: STRICT
```

For the concurrent requests and traffic control you can change the values of these settings

```yaml
apiVersion: networking.istio.io/v1beta1
kind: DestinationRule
metadata:
  name: marketplace-mtls
  namespace: marketplace
spec:
  host: "*.marketplace.svc.cluster.local"
  trafficPolicy:
    tls:
      mode: ISTIO_MUTUAL
    connectionPool:
      tcp:
        maxConnections: 100
      http:
        http1MaxPendingRequests: 10
        maxRequestsPerConnection: 10
        maxRetries: 3
    outlierDetection:
      consecutive5xxErrors: 5
      interval: 30s
      baseEjectionTime: 30s
      maxEjectionPercent: 50
```

This security configuration prevent any access from every clients that wants to access directly to the services without api gateway
even from the kuber itself with another namespace

```yaml
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: internal-services-only
  namespace: marketplace
spec:
  selector:
    matchLabels:
      app: api-gateway
  action: ALLOW
  rules:
    - from:
        - source:
            namespaces: ["marketplace"]
---
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: api-gateway-access
  namespace: marketplace
spec:
  selector:
    matchLabels:
      app: api-gateway
  action: ALLOW
  rules:
    - {}
```