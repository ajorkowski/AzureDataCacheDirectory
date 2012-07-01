pack:
	nuget pack AzureDataCacheDirectory/AzureDataCacheDirectory.csproj
	nuget push AzureDataCacheDirectory.$(v).nupkg
	rm AzureDataCacheDirectory.$(v).nupkg

.PHONY: pack