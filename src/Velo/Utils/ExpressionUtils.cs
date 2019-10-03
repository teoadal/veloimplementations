using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Velo.Utils
{
    public static class ExpressionUtils
    {
        private static readonly Type VoidType = typeof(void);
        private static readonly Expression VoidResult = Expression.Default(VoidType);

        public static Delegate BuildInitializer(Type owner, PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(owner, "instance");
            var property = Expression.Property(instance, propertyInfo);
            
            var constructor = ReflectionUtils.GetConstructor(propertyInfo.PropertyType);
            var assign = Expression.Assign(property, constructor == null
                ? (Expression) Expression.Default(propertyInfo.PropertyType)
                : Expression.New(constructor));

            var body = Expression.Block(
                assign, 
                VoidResult);

            return Expression.Lambda(body, instance).Compile();
        }

        public static Delegate BuildDecrement(Type owner, PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(owner, "instance");
            var property = Expression.Property(instance, propertyInfo);

            var body = Expression.Block(
                Expression.Assign(property, Expression.Decrement(property)),
                VoidResult);

            return Expression.Lambda(body, instance).Compile();
        }

        public static Delegate BuildIncrement(Type owner, PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(owner, "instance");
            var property = Expression.Property(instance, propertyInfo);

            var body = Expression.Block(
                Expression.Assign(property, Expression.Increment(property)),
                VoidResult);

            return Expression.Lambda(body, instance).Compile();
        }

        public static Delegate BuildGetter(Type owner, PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(owner, "instance");

            var body = Expression.Property(instance, propertyInfo);
            return Expression.Lambda(body, instance).Compile();
        }

        public static Delegate BuildSetter(Type owner, PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(owner, "instance");
            var value = Expression.Parameter(propertyInfo.PropertyType, "value");
            var property = Expression.Property(instance, propertyInfo);

            var body = Expression.Block(
                Expression.Assign(property, value),
                VoidResult);

            return Expression.Lambda(body, instance, value).Compile();
        }
    }
}