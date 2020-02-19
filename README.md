# Velo

## Install from nuget

Install Velo with the following command [from nuget](https://www.nuget.org/packages/Velo/):

```ini
Install-Package Velo
```

For integration with IServiceCollection, install Velo.Extensions.DependencyInjection [from nuget](https://www.nuget.org/packages/Velo.Extensions.DependencyInjection):

```ini
Install-Package Velo.Extensions.DependencyInjection
```

## Emitter (mediator)

```cs
var dependencyProvider = new DependencyCollection()
    .AddEmitter()
    .Scan(scanner => scanner // collect all processors and behaviours
        .AssemblyOf<IBooRepository>()
        .AddEmitterProcessors())     
    .BuildProvider();

var emitter = dependencyProvider.GetRequiredService<Emitter>();

// ask query (send requst)
Boo boo = await emitter.Ask(new GetBoo { Id = id }); 

// execute command
await emitter.Execute(new CreateBoo { Id = id }); 

// publish notification
await emitter.Publish(new Notification { Created = true });

// or ask as struct for reduce memory traffic
Boo boo = await emitter.Ask<GetBooStruct, Boo>(new GetBooStruct {Id = id}); 
```

### Registration

```cs
var dependencyProvider = new DependencyCollection()
    .AddCommandBehaviour<MeasureBehaviour>() // add behaviours
    .AddCommandProcessor<PreProcessor>()
    .AddCommandProcessor<Processor>(DependencyLifetime.Scoped) 
    .AddCommandProcessor<PostProcessor>()
    .AddQueryProcessor<QueryPreProcessor>()
    .AddQueryProcessor<QueryProcessor>()
    .AddQueryProcessor<QueryPostProcessor>()
    .AddNotificationProcessor<OnBooCreated>()
```

### Mediator query (request) benchmark (per 1000 requests)

|               Method |      Mean |    Error |   StdDev | Ratio |  Allocated |
|--------------------- |----------:|---------:|---------:|------:|-----------:|
| FullPipeline_MediatR | 805,866.1 ns | 15,881.23 ns | 21,738.41 ns |  1.00 | 1056.07 KB |
|    **FullPipeline_Velo** | 272,772.9 ns |  5,236.39 ns |  6,430.75 ns |  **0.34** |  **256.07 KB** |
|                      |              |              |              |       |           |
|  One_Request_MediatR | 358,579.4 ns |  6,727.49 ns |  7,198.33 ns |  1.00 |  376.07 KB |
|     **One_Request_Velo** | 132,259.3 ns |  2,136.00 ns |  1,998.02 ns |  **0.37** |  **144.07 KB** |

*FullPipeline - behaviour, pre- and post-processor*.

### Mediator notification benchmark (per 1000 notifications)

|  Method |     Mean |   Error |  StdDev | Ratio | Allocated |
|-------- |---------:|--------:|--------:|------:|----------:|
| MediatR | 469.6 us | 9.69 us | 8.59 us |  1.00 |  776 074 B |
| **Velo** | 107.4 us | 1.63 us | 1.44 us |  **0.23** |      **72 B** |

## Mapper

```cs
var compiledMapper = new CompiledMapper<Foo>();

var source = new Boo
{
    Bool = true,
    Float = 1f,
    Int = 11
};

var foo = compiledMapper.Map(source);
```

### Benchmark (per 10000 objects)

|     Method |        Mean |     Error |    StdDev | Ratio |  Allocated |
|----------- |------------:|----------:|----------:|------:|-----------:|
| AutoMapper |    998.9 us |  10.17 us |   9.51 us |  1.00 |  390.63 KB |
| **Velo**   |    299.7 us |   3.06 us |   2.86 us |  **0.30** |  **390.63 KB** |


## Serialization/Deserialization

### Deserialization

```cs
var converter = new JConverter();
var deserialized = converter.Deserialize<Boo[]>(json);
```

### Serialization

```cs
var converter = new JConverter();
var json = converter.Serialize(data);
```

### Serialization benchmark (per 10000 objects)

|     Method |     Mean |    Error |   StdDev | Ratio | Allocated |
|----------- |---------:|---------:|---------:|------:|----------:|
| Newtonsoft | 54.83 ms | 0.567 ms | 0.531 ms |  1.00 |  32.71 MB |
|   **Velo** | 28.71 ms | 0.431 ms | 0.404 ms |  **0.52** |   **9.87 MB** |


### Deserialization benchmark (per 10000 objects)

|     Method |     Mean |    Error |   StdDev | Ratio | Allocated |
|----------- |---------:|---------:|---------:|------:|----------:|
| Newtonsoft | 88.41 ms | 1.575 ms | 1.315 ms |  1.00 |  36.85 MB |
|   **Velo** | 44.28 ms | 0.327 ms | 0.306 ms |  **0.50** |  **19.26 MB** |


## Dependency Injection

### Create container

```cs
var container = new DependencyCollection()
    .AddScoped<SomethingController>()
    .AddSingleton<IFooService, FooService>()
    .AddSingleton(typof(IMapper<>), typeof(CompiledMapper<>))
    .AddSingleton<IConfiguration>(ctx => new Configuration())
    .AddTransient<ISession, Session>()
    .BuildProvider();
```

### Use assembly scanner for find generic interface implementations

```cs
var provider = new DependencyCollection()
    .Scan(scanner => scanner
        .AssemblyOf<IRepository>()
        .SingletoneOf(typeof(IRepository<>)))
    .BuildProvider();
```

### Resolve dependency

```cs
// possible null or empty
var repositoryArray = provider.GetService<IRepository[]>();

// not null or exception
var converterSingleton = provider.GetRequiredService<JConverter>();

// registered as transient
var session = container.GetService<ISession>();
var otherSession = container.GetService<ISession>();
```

### Use scope

```cs
using (var scope = provider.CreateScope())
{
    var controller = scope.GetService<SomethingController>();
}
```

### Benchmarks

#### Create container benchmark

|       Method |       Mean |     Error |    StdDev |  Ratio | Allocated |
|------------- |-----------:|----------:|----------:|-------:|----------:|
|      Autofac |  42.710 us | 0.4666 us | 0.4136 us |  18.28 |   38.9 KB |
|       Castle | 245.270 us | 2.0700 us | 1.9363 us | 105.06 |  91.71 KB |
|         Core |   2.338 us | 0.0254 us | 0.0212 us |   1.00 |   5.54 KB |
|  LightInject |  13.078 us | 0.0809 us | 0.0757 us |   5.60 |  37.45 KB |
| SimpleInject | 401.719 us | 3.0091 us | 2.6675 us | 171.80 |   42.7 KB |
|     **Velo** |   1.739 us | 0.0200 us | 0.0178 us |   **0.74** |   **3.04 KB** |
|        Unity |  15.096 us | 0.2847 us | 0.2796 us |   6.44 |  22.41 KB |


#### Resolve singleton dependency from container

|       Method |        Mean |     Error |    StdDev | Ratio | Allocated |
|------------- |------------:|----------:|----------:|------:|----------:|
|      Autofac |   762.68 ns | 15.218 ns | 25.841 ns |  3.83 |   1 656 B |
|       Castle |   536.30 ns |  5.416 ns |  5.066 ns |  2.71 |   1 200 B |
|         Core |   198.20 ns |  2.320 ns |  2.057 ns |  1.00 |     216 B |
|  LightInject |    85.05 ns |  0.624 ns |  0.584 ns |  0.43 |     216 B |
| SimpleInject |   128.19 ns |  0.942 ns |  0.881 ns |  0.65 |     216 B |
|     **Velo** |   195.92 ns |  2.089 ns |  1.954 ns |  **0.99** |     **216 B** |
|        Unity |   455.58 ns |  3.135 ns |  2.932 ns |  2.30 |     552 B |


## LocalVector (small collection on stack)

### Usage

```cs
var vector = new LocalVector<Boo>();
vector.Add(new Boo()); // add less 10 elements for performance effect
```

### Linq-like via ref struct enumerators

```cs
var outer = new LocalVector<Boo>(_items);
var inner = new LocalVector<Boo>(_reversItems);

var counter = 0;
foreach (var number in outer
    .Join(inner, o => o, i => i, (o, i) => i) 
    .Where((b, threshold) => b.Int > threshold, _threshold) // use argument for avoid clousure
    .Select((b, modifier) => b.Id * modifier, _modifier)
    .OrderBy(id => id))
{
    counter += number;
}

return counter;
```

### Local vector benchmarks

|                                Method | Count |         Mean |       Error |      StdDev |       Median | Ratio | Allocated |
|-------------------------------------- |------ |-------------:|------------:|------------:|-------------:|------:|----------:|
|                              **List_Add** |     **6** |    **70.934 ns** |   **0.8841 ns** |   **0.7838 ns** |    **71.026 ns** |  **1.00** |     **184 B** |
|                       LocalVector_Add |     6 |    35.313 ns |   0.3948 ns |   0.3500 ns |    35.322 ns |  0.50 |         - |
|                              Span_Add |     6 |    25.172 ns |   0.3537 ns |   0.3309 ns |    25.041 ns |  0.35 |      72 B |
|                                       |       |              |             |             |              |       |           |
|                       List_Allocation |     6 |     6.970 ns |   0.1368 ns |   0.1280 ns |     6.964 ns |  1.00 |      18 B |
|                LocalVector_Allocation |     6 |     3.357 ns |   0.0462 ns |   0.0432 ns |     3.341 ns |  0.48 |         - |
|                       Span_Allocation |     6 |     2.458 ns |   0.0336 ns |   0.0314 ns |     2.454 ns |  0.35 |       7 B |
|                                       |       |              |             |             |              |       |           |
|                        List_Iteration |     6 |    90.763 ns |   0.9395 ns |   0.8328 ns |    90.709 ns |  1.00 |     112 B |
|                 LocalVector_Iteration |     6 |    84.531 ns |   1.7477 ns |   3.2826 ns |    84.120 ns |  0.94 |         - |
|                        Span_Iteration |     6 |     8.317 ns |   0.0699 ns |   0.0654 ns |     8.303 ns |  0.09 |         - |
|                                       |       |              |             |             |              |       |           |
|                          List_GroupBy |     6 |   521.716 ns |  10.3672 ns |   9.6975 ns |   521.845 ns |  1.00 |     816 B |
|                   LocalVector_GroupBy |     6 |   697.650 ns |  18.6314 ns |  32.6314 ns |   679.490 ns |  1.36 |         - |
|                                       |       |              |             |             |              |       |           |
|        List_MoreLinq |     6 | 1,747.245 ns |  10.7722 ns |  10.0764 ns | 1,746.922 ns |  1.00 |    1840 B |
| LocalVector_MoreLinq |     6 |   614.650 ns |  14.5216 ns |  41.4311 ns |   592.375 ns |  0.36 |         - |
|                                       |       |              |             |             |              |       |           |
|                           List_Remove |     6 |   216.125 ns |   1.3028 ns |   1.1549 ns |   215.846 ns |  1.00 |     112 B |
|                    LocalVector_Remove |     6 |   185.718 ns |   1.1246 ns |   0.9969 ns |   185.648 ns |  0.86 |         - |
|                                       |       |              |             |             |              |       |           |
|                           List_Select |     6 |   185.795 ns |   2.7152 ns |   2.4070 ns |   184.959 ns |  1.00 |     184 B |
|                    LocalVector_Select |     6 |   146.682 ns |   3.5299 ns |   3.9235 ns |   144.845 ns |  0.79 |         - |
|                                       |       |              |             |             |              |       |           |
|                          List_ToArray |     6 |    87.699 ns |   0.8934 ns |   0.8357 ns |    87.469 ns |  1.00 |     184 B |
|                   LocalVector_ToArray |     6 |    53.201 ns |   0.3991 ns |   0.3538 ns |    53.151 ns |  0.61 |      72 B |
|                          Span_ToArray |     6 |    20.654 ns |   0.1678 ns |   0.1570 ns |    20.612 ns |  0.24 |      72 B |
|                                       |       |              |             |             |              |       |           |
|                            List_Where |     6 |   230.537 ns |   2.0439 ns |   1.7067 ns |   230.668 ns |  1.00 |     184 B |
|                     LocalVector_Where |     6 |   147.815 ns |   2.8671 ns |   2.5416 ns |   147.066 ns |  0.64 |         - |
|                                       |       |              |             |             |              |       |           |
|                    List_Where_ToArray |     6 |   307.613 ns |   1.7394 ns |   1.6271 ns |   307.827 ns |  1.00 |     240 B |
|             LocalVector_Where_ToArray |     6 |   180.913 ns |   3.6185 ns |   5.5258 ns |   179.677 ns |  0.59 |      56 B |
|                                       |       |              |             |             |              |       |           |
|                              **List_Add** |    **10** |   **109.992 ns** |   **0.5531 ns** |   **0.5174 ns** |   **109.963 ns** |  **1.00** |     **336 B** |
|                       LocalVector_Add |    10 |    51.921 ns |   0.3682 ns |   0.3444 ns |    51.865 ns |  0.47 |         - |
|                              Span_Add |    10 |    32.637 ns |   0.1159 ns |   0.1084 ns |    32.643 ns |  0.30 |     104 B |
|                                       |       |              |             |             |              |       |           |
|                       List_Allocation |    10 |    10.952 ns |   0.0963 ns |   0.0901 ns |    10.957 ns |  1.00 |      34 B |
|                LocalVector_Allocation |    10 |     4.894 ns |   0.0289 ns |   0.0270 ns |     4.886 ns |  0.45 |         - |
|                       Span_Allocation |    10 |     3.269 ns |   0.0237 ns |   0.0222 ns |     3.263 ns |  0.30 |      10 B |
|                                       |       |              |             |             |              |       |           |
|                        List_Iteration |    10 |   105.256 ns |   0.6105 ns |   0.5711 ns |   105.231 ns |  1.00 |     144 B |
|                 LocalVector_Iteration |    10 |   119.870 ns |   2.3141 ns |   2.0514 ns |   119.860 ns |  1.14 |         - |
|                        Span_Iteration |    10 |    10.746 ns |   0.2140 ns |   0.2002 ns |    10.814 ns |  0.10 |         - |
|                                       |       |              |             |             |              |       |           |
|                          List_GroupBy |    10 |   738.832 ns |  13.3283 ns |  26.6181 ns |   739.727 ns |  1.00 |    1024 B |
|                   LocalVector_GroupBy |    10 | 1,090.561 ns |  21.6888 ns |  55.2049 ns | 1,071.196 ns |  1.49 |         - |
|                                       |       |              |             |             |              |       |           |
|        List_MoreLinq |    10 | 2,871.348 ns |  61.5866 ns |  68.4533 ns | 2,853.588 ns |  1.00 |    2384 B |
| LocalVector_MoreLinq |    10 | 1,253.259 ns |  24.2574 ns |  27.9349 ns | 1,247.557 ns |  0.44 |         - |
|                                       |       |              |             |             |              |       |           |
|                           List_Remove |    10 |   388.861 ns |   7.7686 ns |   9.2480 ns |   388.455 ns |  1.00 |     144 B |
|                    LocalVector_Remove |    10 |   499.922 ns |   6.9172 ns |   5.7761 ns |   501.994 ns |  1.30 |         - |
|                                       |       |              |             |             |              |       |           |
|                           List_Select |    10 |   278.151 ns |   5.5763 ns |   6.1980 ns |   277.986 ns |  1.00 |     216 B |
|                    LocalVector_Select |    10 |   230.832 ns |   4.4488 ns |   4.9448 ns |   230.019 ns |  0.83 |         - |
|                                       |       |              |             |             |              |       |           |
|                          List_ToArray |    10 |   105.120 ns |   2.4976 ns |   7.2061 ns |   103.721 ns |  1.00 |     248 B |
|                   LocalVector_ToArray |    10 |    84.546 ns |   1.7605 ns |   2.1621 ns |    84.729 ns |  0.73 |     104 B |
|                          Span_ToArray |    10 |    26.499 ns |   0.5832 ns |   0.8176 ns |    26.633 ns |  0.23 |     104 B |
|                                       |       |              |             |             |              |       |           |
|                            List_Where |    10 |   321.476 ns |   6.3157 ns |   9.4531 ns |   321.736 ns |  1.00 |     216 B |
|                     LocalVector_Where |    10 |   228.611 ns |   4.4318 ns |   5.2757 ns |   228.700 ns |  0.71 |         - |
|                                       |       |              |             |             |              |       |           |
|                    List_Where_ToArray |    10 |   473.972 ns |   9.4344 ns |  15.5009 ns |   475.596 ns |  1.00 |     360 B |
|             LocalVector_Where_ToArray |    10 |   256.186 ns |   5.0873 ns |   9.0427 ns |   257.439 ns |  0.54 |      88 B |
|                                       |       |              |             |             |              |       |           |
|                              **List_Add** |    **26** |   **218.854 ns** |   **4.3118 ns** |   **7.2040 ns** |**219.815 ns** | **1.00** |     **616 B** |
|                       LocalVector_Add |    26 |   235.415 ns |   4.6626 ns |   7.3953 ns |   237.527 ns |  1.08 |     296 B |
|                              Span_Add |    26 |    81.367 ns |   1.6209 ns |   3.0045 ns |    82.280 ns |  0.37 |     232 B |
|                                       |       |              |             |             |              |       |           |
|                       List_Allocation |    26 |    21.658 ns |   0.4340 ns |   0.6884 ns |    21.817 ns |  1.00 |      62 B |
|                LocalVector_Allocation |    26 |    22.216 ns |   0.4469 ns |   0.6409 ns |    21.842 ns |  1.03 |      30 B |
|                       Span_Allocation |    26 |     8.289 ns |   0.1667 ns |   0.2495 ns |     8.276 ns |  0.38 |      23 B |
|                                       |       |              |             |             |              |       |           |
|                        List_Iteration |    26 |   190.310 ns |   3.7977 ns |   4.2211 ns |   191.128 ns |  1.00 |     272 B |
|                 LocalVector_Iteration |    26 |   271.090 ns |   5.3886 ns |   8.2290 ns |   272.364 ns |  1.43 |     152 B |
|                        Span_Iteration |    26 |    15.647 ns |   0.3324 ns |   0.3110 ns |    15.682 ns |  0.08 |         - |
|                                       |       |              |             |             |              |       |           |
|                          List_GroupBy |    26 | 1,351.803 ns |  27.0455 ns |  50.7980 ns | 1,337.776 ns |  1.00 |    1456 B |
|                   LocalVector_GroupBy |    26 | 2,294.437 ns |  45.8452 ns |  97.6998 ns | 2,325.693 ns |  1.69 |     544 B |
|                                       |       |              |             |             |              |       |           |
|        List_MoreLinq |    26 | 5,924.927 ns | 115.6725 ns | 146.2887 ns | 5,954.385 ns |  1.00 |    4568 B |
| LocalVector_MoreLinq |    26 | 4,854.369 ns |  95.4935 ns | 117.2746 ns | 4,862.657 ns |  0.82 |     304 B |
|                                       |       |              |             |             |              |       |           |
|                           List_Remove |    26 |   864.966 ns |  17.2935 ns |  25.8841 ns |   871.352 ns |  1.00 |     272 B |
|                    LocalVector_Remove |    26 | 2,630.239 ns |  52.4246 ns |  62.4077 ns | 2,644.387 ns |  3.06 |     152 B |
|                                       |       |              |             |             |              |       |           |
|                           List_Select |    26 |   408.902 ns |   8.1518 ns |  12.9297 ns |   410.874 ns |  1.00 |     344 B |
|                    LocalVector_Select |    26 |   475.331 ns |   9.0550 ns |   8.4700 ns |   476.712 ns |  1.19 |     152 B |
|                                       |       |              |             |             |              |       |           |
|                          List_ToArray |    26 |   119.094 ns |   2.4068 ns |   4.3400 ns |   119.817 ns |  1.00 |     504 B |
|                   LocalVector_ToArray |    26 |   289.539 ns |   5.0091 ns |   4.4404 ns |   288.938 ns |  2.46 |     384 B |
|                          Span_ToArray |    26 |    32.871 ns |   0.7085 ns |   1.2409 ns |    32.854 ns |  0.28 |     232 B |
|                                       |       |              |             |             |              |       |           |
|                            List_Where |    26 |   568.846 ns |   8.5706 ns |   8.0170 ns |   568.138 ns |  1.00 |     344 B |
|                     LocalVector_Where |    26 |   548.809 ns |  10.3536 ns |   9.1782 ns |   551.173 ns |  0.96 |     152 B |
|                                       |       |              |             |             |              |       |           |
|                    List_Where_ToArray |    26 | 1,043.988 ns |  20.5161 ns |  25.1956 ns | 1,044.046 ns |  1.00 |     952 B |
|             LocalVector_Where_ToArray |    26 |   694.211 ns |  13.6259 ns |  12.7457 ns |   696.338 ns |  0.66 |     520 B |
