
$packagePath = "$PSScriptRoot\ServiceFabric\pkg\Debug"
$publishProfilePath = "$PSScriptRoot\ServiceFabric\PublishProfiles\Cloud.xml"


Connect-ServiceFabricCluster -ConnectionEndpoint

. "$PSScriptRoot\ServiceFabric\Scripts\Deploy-FabricApplication.ps1" -ApplicationPackagePath $packagePath -PublishProfileFile $publishProfilePath -DeployOnly:$false -ApplicationParameter:@{} -UnregisterUnusedApplicationVersionsAfterUpgrade $false -OverrideUpgradeBehavior 'None' -OverwriteBehavior 'SameAppTypeAndVersion' -SkipPackageValidation:$false -UseExistingClusterConnection:$false -ErrorAction Stop