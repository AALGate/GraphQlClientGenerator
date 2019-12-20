﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GraphQlClientGenerator.Test
{
    public class GraphQlGeneratorTest
    {
        private static readonly GraphQlSchema TestSchema = DeserializeTestSchema("TestSchema");

        private readonly ITestOutputHelper _outputHelper;

        private static GraphQlSchema DeserializeTestSchema(string resourceName) =>
            GraphQlGenerator.DeserializeGraphQlSchema(GetTestResource(resourceName));

        public GraphQlGeneratorTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            GraphQlGeneratorConfiguration.Reset();
        }

        [Fact]
        public void GenerateQueryBuilder()
        {
            GraphQlGeneratorConfiguration.CustomClassNameMapping.Add("AwayMode", "VacationMode");

            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateQueryBuilder(TestSchema, stringBuilder);

            var expectedQueryBuilders = GetTestResource("ExpectedQueryBuilders");
            stringBuilder.ToString().ShouldBe(expectedQueryBuilders);
        }

        [Fact]
        public void GenerateDataClasses()
        {
            GraphQlGeneratorConfiguration.CustomClassNameMapping.Add("AwayMode", "VacationMode");

            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateDataClasses(TestSchema, stringBuilder);

            var expectedDataClasses = GetTestResource("ExpectedDataClasses");
            stringBuilder.ToString().ShouldBe(expectedDataClasses);
        }

        [Fact]
        public void GenerateDataClassesWithTypeConfiguration()
        {
            GraphQlGeneratorConfiguration.IntegerTypeMapping = IntegerTypeMapping.Int64;
            GraphQlGeneratorConfiguration.FloatTypeMapping = FloatTypeMapping.Double;
            GraphQlGeneratorConfiguration.BooleanTypeMapping = BooleanTypeMapping.Custom;
            GraphQlGeneratorConfiguration.IdTypeMapping = IdTypeMapping.String;
            GraphQlGeneratorConfiguration.GeneratePartialClasses = false;
            GraphQlGeneratorConfiguration.PropertyGeneration = PropertyGenerationOption.BackingField;
            GraphQlGeneratorConfiguration.CustomScalarFieldTypeMapping =
                (baseType, valueType, valueName) => valueType.Name == "Boolean" ? "bool" : GraphQlGeneratorConfiguration.DefaultScalarFieldTypeMapping(baseType, valueType, valueName);

            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateDataClasses(TestSchema, stringBuilder);

            var expectedDataClasses = GetTestResource("ExpectedDataClassesWithTypeConfiguration");
            stringBuilder.ToString().ShouldBe(expectedDataClasses);
        }

        [Fact]
        public void GenerateDataClassesWithInterfaces()
        {
            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateDataClasses(DeserializeTestSchema("TestSchema3"), stringBuilder);

            var expectedDataClasses = GetTestResource("ExpectedDataClassesWithInterfaces");
            stringBuilder.ToString().ShouldBe(expectedDataClasses);
        }

        [Fact]
        public void NewCSharpSyntaxWithClassPostfix()
        {
            GraphQlGeneratorConfiguration.CSharpVersion = CSharpVersion.Newest;
            GraphQlGeneratorConfiguration.ClassPostfix = "V1";
            GraphQlGeneratorConfiguration.MemberAccessibility = MemberAccessibility.Internal;
            var schema = DeserializeTestSchema("TestSchema2");

            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateQueryBuilder(schema, stringBuilder);
            GraphQlGenerator.GenerateDataClasses(schema, stringBuilder);

            var expectedOutput = GetTestResource("ExpectedNewCSharpSyntaxWithClassPostfix");
            var generatedSourceCode = stringBuilder.ToString();
            generatedSourceCode.ShouldBe(expectedOutput);

            CompileIntoAssembly(generatedSourceCode, "GraphQLTestAssembly");

            Type.GetType("GraphQLTestAssembly.GraphQlQueryBuilder, GraphQLTestAssembly").ShouldNotBeNull();
        }

        [Fact]
        public void WithNullableReferences()
        {
            GraphQlGeneratorConfiguration.CSharpVersion = CSharpVersion.NewestWithNullableReferences;
            var schema = DeserializeTestSchema("TestSchema2");

            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateQueryBuilder(schema, stringBuilder);
            GraphQlGenerator.GenerateDataClasses(schema, stringBuilder);

            var expectedOutput = GetTestResource("ExpectedWithNullableReferences");
            var generatedSourceCode = stringBuilder.ToString();
            generatedSourceCode.ShouldBe(expectedOutput);
        }

        [Fact]
        public void GeneratedQuery()
        {
            GraphQlGeneratorConfiguration.JsonPropertyGeneration = JsonPropertyGenerationOption.Always;

            var schema = DeserializeTestSchema("TestSchema2");
            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateQueryBuilder(schema, stringBuilder);
            GraphQlGenerator.GenerateDataClasses(schema, stringBuilder);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(
                @"public class TestQueryBuilder : GraphQlQueryBuilder<TestQueryBuilder>
{
    private static readonly FieldMetadata[] AllFieldMetadata =
        new []
        {
            new FieldMetadata { Name = ""testField"" },
            new FieldMetadata { Name = ""objectParameter"" }
        };

    protected override IList<FieldMetadata> AllFields { get; } = AllFieldMetadata;

    public TestQueryBuilder(string alias = null, QueryBuilderParameter<bool> includeIf = null, QueryBuilderParameter<bool> skipIf = null)
        : base(alias, includeIf, skipIf)
    {
    }

	public TestQueryBuilder WithTestField(
        QueryBuilderParameter<short?> valueInt16 = null,
        QueryBuilderParameter<ushort?> valueUInt16 = null,
        QueryBuilderParameter<byte?> valueByte = null,
        QueryBuilderParameter<int?> valueInt32 = null,
        QueryBuilderParameter<uint?> valueUInt32 = null,
        QueryBuilderParameter<long?> valueInt64 = null,
        QueryBuilderParameter<ulong?> valueUInt64 = null,
        QueryBuilderParameter<float?> valueSingle = null,
        QueryBuilderParameter<double?> valueDouble = null,
        QueryBuilderParameter<decimal?> valueDecimal = null,
        QueryBuilderParameter<DateTime?> valueDateTime = null,
        QueryBuilderParameter<DateTimeOffset?> valueDateTimeOffset = null,
        QueryBuilderParameter<Guid?> valueGuid = null,
        QueryBuilderParameter<string> valueString = null)
	{
		var args = new Dictionary<string, QueryBuilderParameter>();
		if (valueInt16 != null)
			args.Add(""valueInt16"", valueInt16);

		if (valueUInt16 != null)
			args.Add(""valueUInt16"", valueUInt16);

		if (valueByte != null)
			args.Add(""valueByte"", valueByte);

		if (valueInt32 != null)
			args.Add(""valueInt32"", valueInt32);

		if (valueUInt32 != null)
			args.Add(""valueUInt32"", valueUInt32);

		if (valueInt64 != null)
			args.Add(""valueInt64"", valueInt64);

		if (valueUInt64 != null)
			args.Add(""valueUInt64"", valueUInt64);

		if (valueSingle != null)
			args.Add(""valueSingle"", valueSingle);

		if (valueDouble != null)
			args.Add(""valueDouble"", valueDouble);

		if (valueDecimal != null)
			args.Add(""valueDecimal"", valueDecimal);

		if (valueDateTime != null)
			args.Add(""valueDateTime"", valueDateTime);

		if (valueDateTimeOffset != null)
			args.Add(""valueDateTimeOffset"", valueDateTimeOffset);

		if (valueGuid != null)
			args.Add(""valueGuid"", valueGuid);

		if (valueString != null)
			args.Add(""valueString"", valueString);

		return WithScalarField(""testField"", null, null, null, args);
	}

    public TestQueryBuilder WithObjectParameterField(QueryBuilderParameter<object> objectParameter = null)
	{
		var args = new Dictionary<string, QueryBuilderParameter>();
		if (objectParameter != null)
			args.Add(""objectParameter"", objectParameter);

        return WithScalarField(""objectParameter"", ""fieldAlias"", new GraphQlQueryParameter<bool>(""direct"", ""Boolean"", true), (QueryBuilderParameter<bool>)false, args);
    }
}");

            CompileIntoAssembly(stringBuilder.ToString(), "GeneratedQueryTestAssembly");

            var builderType = Type.GetType("GeneratedQueryTestAssembly.TestQueryBuilder, GeneratedQueryTestAssembly");
            builderType.ShouldNotBeNull();
            var formattingType = Type.GetType("GeneratedQueryTestAssembly.Formatting, GeneratedQueryTestAssembly");
            formattingType.ShouldNotBeNull();

            var builderInstance = Activator.CreateInstance(builderType, null, null, null);
            builderType
                .GetMethod("WithTestField", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(
                    builderInstance,
                    new object[]
                    {
                        (short)1,
                        (ushort)2,
                        (byte)3,
                        4,
                        (uint)5,
                        6L,
                        (ulong)7,
                        8.123f,
                        9.456d,
                        10.789m,
                        new DateTime(2019, 6, 30, 0, 27, 47, DateTimeKind.Utc),
                        new DateTimeOffset(2019, 6, 30, 2, 27, 47, TimeSpan.FromHours(2)),
                        Guid.Empty,
                        "string value"
                    }.Select(CreateParameter).ToArray());

            builderType
                .GetMethod("WithObjectParameterField", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(
                    builderInstance,
                    new object[]
                    {
                        new []
                        {
                            JsonConvert.DeserializeObject("{ \"rootProperty1\": \"root value 1\", \"rootProperty2\": 123.456, \"rootProperty3\": true, \"rootProperty4\": null, \"rootProperty5\": { \"nestedProperty\": 987 } }"),
                            JsonConvert.DeserializeObject("[{ \"rootProperty1\": \"root value 2\" }, { \"rootProperty1\": false }]")
                        }
                    }.Select(CreateParameter).ToArray());

            var query =
                builderType
                    .GetMethod("Build", BindingFlags.Instance | BindingFlags.Public)
                    .Invoke(builderInstance, new [] { Enum.Parse(formattingType, "None"), (byte)2 });

            query.ShouldBe("{testField(valueInt16:1,valueUInt16:2,valueByte:3,valueInt32:4,valueUInt32:5,valueInt64:6,valueUInt64:7,valueSingle:8.123,valueDouble:9.456,valueDecimal:10.789,valueDateTime:\"2019-06-30T00:27:47.0000000Z\",valueDateTimeOffset:\"2019-06-30T02:27:47.0000000+02:00\",valueGuid:\"00000000-0000-0000-0000-000000000000\",valueString:\"string value\"),fieldAlias:objectParameter(objectParameter:[{rootProperty1:\"root value 1\",rootProperty2:123.456,rootProperty3:true,rootProperty4:null,rootProperty5:{nestedProperty:987}},[{rootProperty1:\"root value 2\"},{rootProperty1:false}]])@include(if:$direct)@skip(if:false)}");
            query =
                builderType
                    .GetMethod("Build", BindingFlags.Instance | BindingFlags.Public)
                    .Invoke(builderInstance, new[] { Enum.Parse(formattingType, "Indented"), (byte)2 });

            query.ShouldBe($" {{{Environment.NewLine}  testField(valueInt16: 1, valueUInt16: 2, valueByte: 3, valueInt32: 4, valueUInt32: 5, valueInt64: 6, valueUInt64: 7, valueSingle: 8.123, valueDouble: 9.456, valueDecimal: 10.789, valueDateTime: \"2019-06-30T00:27:47.0000000Z\", valueDateTimeOffset: \"2019-06-30T02:27:47.0000000+02:00\", valueGuid: \"00000000-0000-0000-0000-000000000000\", valueString: \"string value\"){Environment.NewLine}  fieldAlias: objectParameter(objectParameter: [{Environment.NewLine}    {{{Environment.NewLine}      rootProperty1: \"root value 1\",{Environment.NewLine}      rootProperty2: 123.456,{Environment.NewLine}      rootProperty3: true,{Environment.NewLine}      rootProperty4: null,{Environment.NewLine}      rootProperty5: {{{Environment.NewLine}        nestedProperty: 987}}}},{Environment.NewLine}    [{Environment.NewLine}    {{{Environment.NewLine}      rootProperty1: \"root value 2\"}},{Environment.NewLine}    {{{Environment.NewLine}      rootProperty1: false}}]]) @include(if: $direct) @skip(if: false){Environment.NewLine}}}");

            static object CreateParameter(object value)
            {
                var genericType = value.GetType();
                if (genericType.IsValueType)
                    genericType = typeof(Nullable<>).MakeGenericType(value.GetType());

                if (value is object[])
                    genericType = typeof(object);

                var makeGenericType = Type.GetType("GeneratedQueryTestAssembly.QueryBuilderParameter`1, GeneratedQueryTestAssembly").MakeGenericType(genericType);
                var parameter = Activator.CreateInstance(makeGenericType, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { value }, CultureInfo.InvariantCulture);
                return parameter;
            }

            builderType = Type.GetType("GeneratedQueryTestAssembly.QueryQueryBuilder, GeneratedQueryTestAssembly");
            builderType.ShouldNotBeNull();
            builderInstance = builderType.GetConstructor(new [] { typeof(string) }).Invoke(new object[1]);
            builderType.GetMethod("WithAllFields", BindingFlags.Instance | BindingFlags.Public).Invoke(builderInstance, null);
            builderType
                .GetMethod("Build", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(builderInstance, new[] { Enum.Parse(formattingType, "None"), (byte)2 });
        }

        [Fact]
        public void DeprecatedAttributes()
        {
            GraphQlGeneratorConfiguration.CSharpVersion = CSharpVersion.Newest;
            GraphQlGeneratorConfiguration.CommentGeneration = CommentGenerationOption.CodeSummary | CommentGenerationOption.DescriptionAttribute;
            GraphQlGeneratorConfiguration.IncludeDeprecatedFields = true;
            GraphQlGeneratorConfiguration.GeneratePartialClasses = false;
            var schema = DeserializeTestSchema("TestSchemaWithDeprecatedFields");

            var stringBuilder = new StringBuilder();
            GraphQlGenerator.GenerateDataClasses(schema, stringBuilder);
            var expectedOutput = GetTestResource("ExpectedDeprecatedAttributes").Replace("\r", String.Empty);
            stringBuilder.ToString().Replace("\r", String.Empty).ShouldBe(expectedOutput);
        }

        private static string GetTestResource(string name)
        {
            using var reader = new StreamReader(typeof(GraphQlGeneratorTest).Assembly.GetManifestResourceStream($"GraphQlClientGenerator.Test.{name}"));
            return reader.ReadToEnd();
        }

        private static void CompileIntoAssembly(string sourceCode, string assemblyName)
        {
            var syntaxTree =
                SyntaxFactory.ParseSyntaxTree(
                    $@"{GraphQlGenerator.RequiredNamespaces}

namespace {assemblyName}
{{
{sourceCode}
}}",
                    CSharpParseOptions.Default.WithLanguageVersion(Enum.GetValues(typeof(LanguageVersion)).Cast<LanguageVersion>().Max()));

            var compilationOptions =
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithPlatform(Platform.AnyCpu)
                    .WithOverflowChecks(true)
                    .WithOptimizationLevel(OptimizationLevel.Release);

            var systemReference = MetadataReference.CreateFromFile(typeof(DateTimeOffset).Assembly.Location);
            var systemObjectModelReference = MetadataReference.CreateFromFile(Assembly.Load("System.ObjectModel").Location);
            var systemTextRegularExpressionsReference = MetadataReference.CreateFromFile(Assembly.Load("System.Text.RegularExpressions").Location);
            var runtimeReference = MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location);
            var netStandardReference = MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location);
            var linqReference = MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location);
            var linqExpressionsReference = MetadataReference.CreateFromFile(Assembly.Load("System.Linq.Expressions").Location);
            var jsonNetReference = MetadataReference.CreateFromFile(Assembly.Load("Newtonsoft.Json").Location);
            var runtimeSerializationReference = MetadataReference.CreateFromFile(typeof(EnumMemberAttribute).Assembly.Location);
            var componentModelReference = MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location);
            var componentModelTypeConverterReference = MetadataReference.CreateFromFile(Assembly.Load("System.ComponentModel.TypeConverter").Location);

            var compilation =
                CSharpCompilation.Create(
                    assemblyName,
                    new[] { syntaxTree },
                    new[]
                    {
                        systemReference,
                        runtimeSerializationReference,
                        systemObjectModelReference,
                        systemTextRegularExpressionsReference,
                        componentModelReference,
                        componentModelTypeConverterReference,
                        runtimeReference,
                        jsonNetReference,
                        linqReference,
                        linqExpressionsReference,
                        netStandardReference
                    },
                    compilationOptions);

            var assemblyFileName = Path.GetTempFileName();
            var result = compilation.Emit(assemblyFileName);
            var errorReport = String.Join(Environment.NewLine, result.Diagnostics.Where(l => l.Severity != DiagnosticSeverity.Hidden).Select(l => $"[{l.Severity}] {l.ToString()}"));
            errorReport.ShouldBeNullOrEmpty();

            Assembly.LoadFrom(assemblyFileName);
        }
    }
}
