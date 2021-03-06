using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Velo.DependencyInjection;
using Velo.Serialization.Tokenization;

namespace Velo.Utils
{
    internal static class Error
    {
        public static ArgumentException AlreadyExists(string? message = null)
        {
            return new ArgumentException(message ?? "Element already exists");
        }

        public static TypeAccessException CircularDependency(Type contract)
        {
            return new TypeAccessException($"Detected circular dependency '{ReflectionUtils.GetName(contract)}'");
        }

        public static TypeAccessException CircularDependency(string message)
        {
            return new TypeAccessException(message);
        }

        public static InvalidCastException Cast(string message, Exception? innerException = null)
        {
            return new InvalidCastException(message, innerException);
        }

        public static KeyNotFoundException DependencyNotRegistered(Type contract)
        {
            var name = ReflectionUtils.GetName(contract);
            return new KeyNotFoundException($"Dependency with contract '{name}' is not registered");
        }

        public static KeyNotFoundException DependencyNotRegistered(string message)
        {
            return new KeyNotFoundException(message);
        }

        public static KeyNotFoundException DefaultConstructorNotFound(Type type)
        {
            return new KeyNotFoundException($"Default constructor for '{ReflectionUtils.GetName(type)}' not found");
        }

        public static SerializationException Deserialization(string message)
        {
            return new SerializationException(message);
        }

        public static SerializationException Deserialization(JsonTokenType expected, JsonTokenType actual)
        {
            return new SerializationException($"Expected {expected} json token, but found {actual} token");
        }

        public static ObjectDisposedException Disposed(string objectName)
        {
            return new ObjectDisposedException(objectName);
        }

        public static InvalidOperationException InconsistentLifetime(
            Type dependedType,
            DependencyLifetime dependedLifetime,
            Type dependencyType,
            DependencyLifetime dependencyLifetime)
        {
            return new InvalidOperationException(
                $"Type of {ReflectionUtils.GetName(dependedType)} with lifetime {dependedLifetime} " +
                $"depended on {ReflectionUtils.GetName(dependencyType)} with lifetime {dependencyLifetime}");
        }

        public static InvalidDataException InvalidData(string message)
        {
            return new InvalidDataException(message);
        }

        public static InvalidDataException InvalidDependencyLifetime(string? message = null)
        {
            return new InvalidDataException(message ?? "Invalid dependency lifetime");
        }

        public static InvalidOperationException InvalidOperation(string message)
        {
            return new InvalidOperationException(message);
        }

        public static FileNotFoundException FileNotFound(string path)
        {
            return new FileNotFoundException($"Required file '{path}' not found", Path.GetFileName(path));
        }

        public static IndexOutOfRangeException OutOfRange(string? message = null)
        {
            return new IndexOutOfRangeException(message ?? "Index out of range");
        }

        public static KeyNotFoundException NotFound(string? message = null)
        {
            return new KeyNotFoundException(message ?? "Element not found");
        }

        public static NotSupportedException NotSupported(string? message = null)
        {
            message ??= "Operation not supported in current context";
            return new NotSupportedException(message);
        }

        public static ArgumentNullException Null(string argumentName)
        {
            return new ArgumentNullException(argumentName);
        }
    }
}