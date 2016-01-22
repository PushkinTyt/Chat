using System;
using System.ServiceModel;

[ServiceContract]
interface ICacheService
{
    [OperationContract]
    void notifyReferation(string URL);
    [OperationContract]
    bool cacheFileExists(string URL);
    [OperationContract]
    string getCachedFile(string URL);
}
