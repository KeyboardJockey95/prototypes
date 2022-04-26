using System;

namespace JTLanguageModelsPortable.Application
{
    public interface ICookieContainer
    {
        bool Exists(string key);
        string GetValue(string key);
        T GetValue<T>(string key);
        void SetValue(string key, object value, DateTime expires);
        void Delete(string key);

        bool RawCookieExists(string key);
        string RawCookieGetValue(string key);
        T RawCookieGetValue<T>(string key);
        void RawCookieSetValue(string key, object value, DateTime expires);
        void RawCookieDelete(string key);
    }
}
