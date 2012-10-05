pack:
	nuget pack AzureDataCacheDirectory/AzureDataCacheDirectory.csproj -Prop Configuration=Release
	nuget push AzureDataCacheDirectory.$(v).nupkg
	rm AzureDataCacheDirectory.$(v).nupkg

.PHONY: pack