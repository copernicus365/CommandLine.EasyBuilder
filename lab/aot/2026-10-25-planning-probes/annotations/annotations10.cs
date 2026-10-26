#:property TargetFramework=net10.0
#:property PublishAot=false

// Probe: which trim/AOT attributes the runtime declares on the APIs the EasyBuilder plan might use.

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Console;

WriteLine(RuntimeInformation.FrameworkDescription);
WriteLine();

Show("Array.CreateInstance(Type, int)", typeof(Array).GetMethod("CreateInstance", [typeof(Type), typeof(int)]));
Show("Array.CreateInstanceFromArrayType(Type, int)", typeof(Array).GetMethod("CreateInstanceFromArrayType", [typeof(Type), typeof(int)]));
Show("Type.MakeGenericType(Type[])", typeof(Type).GetMethod("MakeGenericType", [typeof(Type[])]));
Show("Type.MakeArrayType()", typeof(Type).GetMethod("MakeArrayType", Type.EmptyTypes));
Show("MethodInfo.MakeGenericMethod(Type[])", typeof(MethodInfo).GetMethod("MakeGenericMethod", [typeof(Type[])]));
Show("Enum.GetValues(Type)", typeof(Enum).GetMethod("GetValues", [typeof(Type)]));
Show("Enum.GetNames(Type)", typeof(Enum).GetMethod("GetNames", [typeof(Type)]));
Show("Enum.TryParse(Type, string, bool, out object)", typeof(Enum).GetMethod("TryParse", [typeof(Type), typeof(string), typeof(bool), typeof(object).MakeByRefType()]));
Show("Enum.ToObject(Type, object)", typeof(Enum).GetMethod("ToObject", [typeof(Type), typeof(object)]));
Show("Activator.CreateInstance(Type)", typeof(Activator).GetMethod("CreateInstance", [typeof(Type)]));
Show("Activator.CreateInstance(Type, object[])", typeof(Activator).GetMethod("CreateInstance", [typeof(Type), typeof(object[])]));
Show("Expression.Lambda(Type, Expression, ParameterExpression[])", typeof(Expression).GetMethod("Lambda", [typeof(Type), typeof(Expression), typeof(ParameterExpression[])]));
Show("LambdaExpression.Compile()", typeof(LambdaExpression).GetMethod("Compile", Type.EmptyTypes));

PropertyInfo dyn = typeof(RuntimeFeature).GetProperty("IsDynamicCodeSupported");
WriteLine("RuntimeFeature.IsDynamicCodeSupported");
foreach(CustomAttributeData a in dyn.CustomAttributes.Concat(dyn.GetMethod.CustomAttributes))
	WriteLine($"    {Describe(a)}");
WriteLine();

foreach(string t in new[] { "System.Diagnostics.CodeAnalysis.FeatureGuardAttribute", "System.Diagnostics.CodeAnalysis.FeatureSwitchDefinitionAttribute" })
	WriteLine($"{t}: {(typeof(object).Assembly.GetType(t) != null ? "exists" : "absent")}");

void Show(string label, MethodInfo mi)
{
	WriteLine(label);
	if(mi == null) {
		WriteLine("    (method absent on this runtime)");
		WriteLine();
		return;
	}

	CustomAttributeData[] attrs = [.. mi.CustomAttributes.Where(IsTrimAot)];
	if(attrs.Length == 0)
		WriteLine("    (no trim/AOT attributes on the method)");

	foreach(CustomAttributeData a in attrs)
		WriteLine($"    {Describe(a)}");

	foreach(ParameterInfo p in mi.GetParameters())
		foreach(CustomAttributeData a in p.CustomAttributes.Where(IsTrimAot))
			WriteLine($"    param '{p.Name}': {Describe(a)}");

	WriteLine();
}

static bool IsTrimAot(CustomAttributeData a)
	=> a.AttributeType.Namespace == "System.Diagnostics.CodeAnalysis"
	&& a.AttributeType.Name is not ("NotNullWhenAttribute" or "NotNullAttribute" or "MaybeNullWhenAttribute" or "AllowNullAttribute");

static string Describe(CustomAttributeData a)
{
	string args = string.Join(", ", a.ConstructorArguments.Select(c => c.Value is string s ? $"\"{s}\"" : c.Value?.ToString()));
	return $"[{a.AttributeType.Name.Replace("Attribute", "")}({args})]";
}
