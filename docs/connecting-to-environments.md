# Using Kubernetes to connect to dev/pre-prod environments

If you need to connect to one of the environments, for example to create yourself an admin account, you'll need to get Azure CLI and Kubernetes set up locally.
**Note:** if you're on Windows, it's recommended to use Windows command prompt rather than git bash for this as git bash is quite flaky.

1. Install [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest&pivots=msi)
1. Log in to Azure locally:

   ```shell
   > az login --tenant 9c7d9dd3-840c-4b3f-818e-552865082e16
   Select the account you want to log in with. For more information on login with Azure CLI, see https://go.microsoft.com/fwlink/?linkid=2271136
   ```
1. Choose your account and authenticate
1. Select the default subscription to the subscription you want to access, e.g. `s189-teacher-services-cloud-test` (`20da9d12-7ee1-42bb-b969-3fe9112964a7`)

   ```shell
   Retrieving tenants and subscriptions for the selection...
   The following tenants don't contain accessible subscriptions. Use `az login --allow-no-subscriptions` to have tenant level access.
   fad277c9-c60a-4da1-b5f3-b3b8b34a82f9 'Department for Education'
   
   [Tenant and subscription selection]
   
   No     Subscription name                     Subscription ID                       Tenant
   -----  ------------------------------------  ------------------------------------  ------------------------------------
   [1]    N/A(tenant level account)             fad277c9-c60a-4da1-b5f3-b3b8b34a82f9  fad277c9-c60a-4da1-b5f3-b3b8b34a82f9

   [...]

   [15]   s189-teacher-services-cloud-devel...  5c83eb53-a94f-4778-b258-1f33efe49655  DfE Platform Identity
   [16]   s189-teacher-services-cloud-produ...  3c033a0c-7a1c-4653-93cb-0f2a9f57a391  DfE Platform Identity
   [17]   s189-teacher-services-cloud-test      20da9d12-7ee1-42bb-b969-3fe9112964a7  DfE Platform Identity   <-- this one
   
   The default is marked with an *; the default tenant is 'DfE Platform Identity' and subscription is [...].
   
   Select a subscription and tenant (Type a number or Enter for no changes): 17
   ```

   Alternatively, do

   ```shell
   > az account set --subscription s189-teacher-services-cloud-test
   ```

1. Pull down the credentials to authenticate with Kubernetes, in this case for dev we want `s189t01-tsc-test-aks` in resource group `s189t01-tsc-ts-rg` (YMMV)

   ```shell
   > az aks get-credentials --overwrite-existing -g s189t01-tsc-ts-rg --name s189t01-tsc-test-aks
   Merged "s189t01-tsc-test-aks" as current context in <your user directory>\.kube\config
   The kubeconfig uses devicecode authentication which requires kubelogin. Please install kubelogin from https://github.com/Azure/kubelogin or run 'az aks install-cli' to install both kubectl and kubelogin. If devicecode login fails, try running 'kubelogin convert-kubeconfig -l azurecli' to unblock yourself.
   ```

1. As the output suggests, install kubectl/kubeconfig via `az aks install`  - this didn't work for me due to certificate issues, so manually:
   1. [install kubectl](https://cjyabraham.gitlab.io/docs/tasks/tools/install-kubectl/#install-kubectl) - on Windows, download the binary directly from [this link](https://storage.googleapis.com/kubernetes-release/release/v1.11.0/bin/windows/amd64/kubectl.exe) and add it to your PATH
   1. [install kubelogin](https://github.com/Azure/kubelogin) - on Windows:
      ```shell
      > winget install --id=Kubernetes.kubectl  -e
      > winget install --id=Microsoft.Azure.Kubelogin  -e
      ```

   1. Restart the command window to pick up the additions to PATH

1. Convert kubeconfig to Exec plugin ([more info](https://github.com/Azure/kubelogin/blob/main/docs/book/src/cli/convert-kubeconfig.md)):
1. ```shell
   > kubelogin convert-kubeconfig -l azurecli
   ```
1. Now you should be able to connect to the Kubernetes cluster. To see the pods available to connect to call `get pods` (if you see a certificate error you might need to add the `--insecure-skip-tls-verify` argument):

   ```shell
   > kubectl get pods -n tra-development --insecure-skip-tls-verify
   NAME                                              READY     STATUS      RESTARTS       AGE
   [...]                                             
   trs-dev-api-7bd486dcdc-z5zvw                      1/1       Running     0              2m18s
   trs-dev-authz-6cd4545d94-fwfg7                    1/1       Running     0              2m18s
   trs-dev-migrations-gvvpb                          0/1       Completed   0              2m46s
   trs-dev-ui-6969874cc9-j96mz                       1/1       Running     0              2m18s
   trs-dev-worker-7444b9cd96-v52bk                   1/1       Running     0              2m17s
   ```
1. Indentify the pod you want to connect to, in this case we want the Dev UI to create an admin account: `trs-dev-ui-6969874cc9-j96mz`
1. Execute a bash shell on the pod (again, add `--insecure-skip-tls-verify` if there are cert issues):

   ```shell
   > kubectl exec -it trs-dev-ui-6969874cc9-j96mz -n tra-development --insecure-skip-tls-verify -- /bin/ash
   ```

1. If you need to create yourself an admin account (e.g. for a brand new environment), use the TRS CLI:

   ```shell
   $ trscli create-admin --email your.email@education.gov.uk --name "Your Name"
   ```

## Connecting to Preprod
Preprod is in the same cluster as dev, so the first few steps above will be the same (shown here for completion, if you already did this for dev above you won't need to do it again for preprod)

```shell
> az account set --subscription s189-teacher-services-cloud-test
> az aks get-credentials --overwrite-existing -g s189t01-tsc-ts-rg --name s189t01-tsc-test-aks
```

For preprod the namespace is `tra-test`:

```shell
> kubectl get pods -n tra-test --insecure-skip-tls-verify
NAME                                              READY     STATUS      RESTARTS       AGE
[...]                                             
trs-pre-production-api-96b85cc55-2tldp                            1/1       Running     0               5m53s
trs-pre-production-authz-54cb944b45-jqnsj                         1/1       Running     0               5m53s
trs-pre-production-migrations-wtnrj                               0/1       Completed   0               6m22s
trs-pre-production-ui-7fddbdf6ff-sgcrm                            1/1       Running     0               5m54s
trs-pre-production-worker-5cfb95c7cd-8rlkd                        1/1       Running     6 (2m45s ago)   5m53s
> kubectl exec -it trs-pre-production-ui-7fddbdf6ff-sgcrm -n tra-test --insecure-skip-tls-verify -- /bin/ash
```

## Connecting to Prod
To connect to prod you will need to create a PIM request for the `s189 TRA production PIM` group at Home -> Privileged Identity Management -> My Roles -> Groups (if not already done previously).

Prod is in the production cluster, in the `s189-teacher-services-cloud-production` subscription:

```shell
> az account set --subscription s189-teacher-services-cloud-production
> az aks get-credentials --overwrite-existing -g s189p01-tsc-pd-rg --name s189p01-tsc-production-aks
```

For prod the namespace is `tra-production`:

```shell
> kubectl get pods -n tra-production --insecure-skip-tls-verify
NAME                                              READY     STATUS      RESTARTS       AGE
[...]                                             
trs-production-api-7b868967d-42r4b                                1/1       Running            0                57m
trs-production-authz-759dbfd488-ck97l                             1/1       Running            0                57m
trs-production-migrations-f6z47                                   0/1       Completed          0                57m
trs-production-ui-88865db7d-8wcnk                                 1/1       Running            0                56m
trs-production-worker-77f4749f64-q9fq8                            0/1       CrashLoopBackOff   15 (4m59s ago)   57m
> kubectl exec -it trs-production-ui-88865db7d-8wcnk -n tra-production --insecure-skip-tls-verify -- /bin/ash
```

## Connecting to TPS sandbox

TPS sandbox pods are in the prod cluster as well, in the `tra-production` namespace:

```shell
> az account set --subscription s189-teacher-services-cloud-production
> az aks get-credentials --overwrite-existing -g s189p01-tsc-pd-rg --name s189p01-tsc-production-aks
> kubectl get pods -n tra-production --insecure-skip-tls-verify
NAME                                              READY     STATUS      RESTARTS       AGE
[...]                                             
trs-tps-sandbox-api-f6b77f97-bgtsw                                1/1       Running            0                54m
trs-tps-sandbox-authz-7dd9c48b5-68tcq                             1/1       Running            0                54m
trs-tps-sandbox-migrations-7g2fn                                  0/1       Completed          0                55m
trs-tps-sandbox-ui-64b78b5c-w6grn                                 1/1       Running            0                54m
trs-tps-sandbox-worker-599fb4c7cc-pbltb                           1/1       Running            0                54m
> kubectl exec -it trs-tps-sandbox-ui-64b78b5c-w6grn -n tra-production --insecure-skip-tls-verify -- /bin/ash
```

## Connecting to pentest environment (for disaster recovery)

s189-teacher-services-cloud-test
s189t01-tsc-pt-rg
s189t01-tsc-platform-test-aks

```shell
> az account set --subscription s189-teacher-services-cloud-test
> az aks get-credentials --overwrite-existing -g s189t01-tsc-pt-rg --name s189t01-tsc-platform-test-aks
> kubectl get pods -n development --insecure-skip-tls-verify
NAME                                              READY     STATUS      RESTARTS       AGE
[...]                                             
trs-pentest-api-85b757cc86-cvwqg               1/1       Running     0          3d1h
trs-pentest-authz-579bf4d698-q7z8r             1/1       Running     0          3d1h
trs-pentest-migrations-fwsrd                   0/1       Completed   0          3d1h
trs-pentest-ui-7dcb6d5fbd-7ph2b                1/1       Running     0          3d1h
trs-pentest-worker-566cffc6dd-jkxt7            1/1       Running     0          3d1h
> kubectl exec -it trs-pentest-ui-7dcb6d5fbd-7ph2b -n development --insecure-skip-tls-verify -- /bin/ash
```

## Connecting to other environments

To connect to other environments see if you can find the relevant information in the Makefile or terraform config.

# Using Kubernetes to connect to a database

To connect to a database, first connect to a pod as specified above, and then run `./db.sh`:

```shell
> kubectl exec -it trs-production-ui-88865db7d-8wcnk -n tra-production --insecure-skip-tls-verify -- /bin/ash
$ ./db.sh
psql (17.6)
SSL connection (protocol: TLSv1.3, cipher: TLS_AES_256_GCM_SHA384, compression: off, ALPN: postgresql)
Type "help" for help.

trs_production=>
```

## Exporting the results of a query

To save the results of a query on another environment you can use the psql `\copy` command:

```shell
trs_production=> \copy (select * from persons limit 10) to '/tmp/results.csv' with csv delimiter ',' header;
```

Only the `/tmp` directory has write permissions so make sure to specify that in the destination path.

Then you can copy the file down using `kubectl cp` (obviously specifying the correct pod and namespace):
```shell
> kubectl cp trs-production-ui-7644c65bc-8vbjm:/tmp/results.csv /local/path/results.csv -n tra-production --insecure-skip-tls-verify
```
