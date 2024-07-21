using System;

namespace AudioStorageSourceGenerator;

[System.Diagnostics.Conditional("AudioStorage_Attributes")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AudioStorageAttribute(string storageName, string namespaceName, string audioFolder) : Attribute
{
    public string StorageName { get; } = storageName;
    public string NamespaceName { get; } = namespaceName;
    public string LocalAudioFolder { get; } = audioFolder;
}
