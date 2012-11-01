# AzureDataCacheDirectory

This is an implementation of the Lucene Directory so that it's backend is the new preview shared cache.

NOTE: It must use the preview version of the local cache (ie not the external azure cache) so that regions are available

Updated to use V2 of the azure libraries and V3 of lucene

## Get it

Install-Package AzureDataCacheDirectory

## Usage

```
// Open your datacache - This should be a persistant store to be useful... (ie no expiration of keys by default)
var dataCache = new DataCache("Persistant");

// Create the directory, the string here is the Region that this will work in
// Note that regions are limited to a single instance in azure cache... so make sure you
// dont index everything on one region, and realise there is an upper limit on the indexes
var directory = new AzureDataCacheDirectory("TermFreeText", cache);
            
// You can see if you have any indexes in your region
// ie you can check on global.asax if you need to spin up your indexes
directory.HasIndexes();

// You can clear the indexes you currently have
directory.ClearAllIndexes();

// Write your indexes from Lucene as per normal
var analyser = new StandardAnalyzer(_version);
var writer = new IndexWriter(directory, analyser, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);
...

// Query from lucene as per normal
var searcher = new IndexSearcher(directory, true);
...
```

## What's the point?
I use this as a backend for my bounded search set. I know it will fit comfortably inside the cache and 
it won't get any bigger. This situation is perfect for usage in the cache as we can utilise the compute power
we have at our disposal and don't have to worry about blob storage etc...

## License
MIT Licensed