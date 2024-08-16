// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "I prefer using x.ToArray() over [.. x]")]
[assembly: SuppressMessage("Style", "IDE0053:Use expression body for lambda expression", Justification = "I want block bodies for lambdas with side effects", Scope = "member", Target = "~M:Funciton.FuncitonCompiler.convertToInstructions(Mono.Cecil.MethodDefinition,System.Func{Mono.Cecil.TypeReference,Mono.Cecil.Cil.VariableDefinition},System.Boolean,System.Object[])")]
