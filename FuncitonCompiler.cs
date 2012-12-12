using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace FuncitonInterpreter
{
    class FuncitonCompiler
    {
        public static void CompileTo(FuncitonProgram program, string targetFilePath)
        {
            new FuncitonCompiler(program, Path.GetFileNameWithoutExtension(targetFilePath)).Assembly.Write(targetFilePath);
        }

        private AssemblyDefinition _asm;
        private ModuleDefinition _mod;
        private MethodDefinition _stdinMethod;
        private TypeDefinition _interface;
        private MethodDefinition _interfaceMethod;

        private TypeReference _void;
        private TypeReference _bool;
        private TypeReference _int;
        private TypeReference _string;
        private TypeReference _object;
        private TypeReference _bigInteger;
        private MethodReference _bigInteger_Parse;
        private MethodReference _bigInteger_get_MinusOne;
        private MethodReference _bigInteger_get_IsZero;
        private MethodReference _bigInteger_get_Zero;
        private MethodReference _bigInteger_op_Implicit_int;
        private MethodReference _bigInteger_op_Implicit_long;
        private MethodReference _bigInteger_op_LeftShift;
        private MethodReference _bigInteger_op_RightShift;
        private MethodReference _bigInteger_op_BitwiseOr;
        private MethodReference _bigInteger_op_BitwiseAnd;
        private MethodReference _bigInteger_op_OnesComplement;
        private MethodReference _bigInteger_op_LessThan;
        private MethodReference _bigInteger_op_Equality;
        private MethodReference _bigInteger_op_Explicit_toInt;
        private MethodReference _char_ConvertToUtf32;
        private MethodReference _char_IsSurrogate;
        private MethodReference _console_set_InputEncoding;
        private MethodReference _console_get_In;
        private MethodReference _encoding_get_UTF8;
        private MethodReference _object_ctor;
        private MethodReference _string_get_Length;
        private MethodReference _string_get_Chars;
        private MethodReference _textReader_ReadToEnd;

        private Dictionary<FuncitonFunction, FunctionTypeInfo> _functionTypes;
        private Dictionary<FuncitonFunction.Node, NodeInfo> _nodeInfos;
        private Dictionary<FuncitonFunction.CallNode, CallNodeInfo> _callNodeFields;
        private Dictionary<FuncitonFunction.InputNode, FieldDefinition> _inputFields;

        private const System.Reflection.BindingFlags _publicStatic = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
        private const System.Reflection.BindingFlags _publicInstance = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;

        private FuncitonCompiler(FuncitonProgram program, string name)
        {
            _asm = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, new Version(1, 0)), name, ModuleKind.Console);
            _mod = _asm.MainModule;

            _void = _mod.Import(typeof(void));
            _bool = _mod.Import(typeof(bool));
            _int = _mod.Import(typeof(int));
            _string = _mod.Import(typeof(string));
            _object = _mod.Import(typeof(object));
            _bigInteger = _mod.Import(typeof(BigInteger));
            _object_ctor = _mod.Import(typeof(object).GetConstructor(Type.EmptyTypes));
            _bigInteger_op_Implicit_long = _mod.Import(typeof(BigInteger).GetMethod("op_Implicit", _publicStatic, null, new[] { typeof(long) }, null));
            _bigInteger_Parse = _mod.Import(typeof(BigInteger).GetMethod("Parse", _publicStatic, null, new[] { typeof(string) }, null));
            _bigInteger_get_Zero = _mod.Import(typeof(BigInteger).GetMethod("get_Zero", _publicStatic, null, Type.EmptyTypes, null));
            _bigInteger_get_MinusOne = _mod.Import(typeof(BigInteger).GetMethod("get_MinusOne", _publicStatic, null, Type.EmptyTypes, null));
            _encoding_get_UTF8 = _mod.Import(typeof(Encoding).GetMethod("get_UTF8", _publicStatic, null, Type.EmptyTypes, null));
            _console_set_InputEncoding = _mod.Import(typeof(Console).GetMethod("set_InputEncoding", _publicStatic, null, new[] { typeof(Encoding) }, null));
            _console_get_In = _mod.Import(typeof(Console).GetMethod("get_In", _publicStatic, null, Type.EmptyTypes, null));
            _textReader_ReadToEnd = _mod.Import(typeof(TextReader).GetMethod("ReadToEnd", _publicInstance, null, Type.EmptyTypes, null));
            _string_get_Length = _mod.Import(typeof(string).GetMethod("get_Length", _publicInstance, null, Type.EmptyTypes, null));
            _char_ConvertToUtf32 = _mod.Import(typeof(char).GetMethod("ConvertToUtf32", _publicStatic, null, new[] { typeof(string), typeof(int) }, null));
            _bigInteger_op_Implicit_int = _mod.Import(typeof(BigInteger).GetMethod("op_Implicit", _publicStatic, null, new[] { typeof(int) }, null));
            _bigInteger_op_LeftShift = _mod.Import(typeof(BigInteger).GetMethod("op_LeftShift", _publicStatic, null, new[] { typeof(BigInteger), typeof(int) }, null));
            _bigInteger_op_RightShift = _mod.Import(typeof(BigInteger).GetMethod("op_RightShift", _publicStatic, null, new[] { typeof(BigInteger), typeof(int) }, null));
            _bigInteger_op_BitwiseOr = _mod.Import(typeof(BigInteger).GetMethod("op_BitwiseOr", _publicStatic, null, new[] { typeof(BigInteger), typeof(BigInteger) }, null));
            _bigInteger_op_BitwiseAnd = _mod.Import(typeof(BigInteger).GetMethod("op_BitwiseAnd", _publicStatic, null, new[] { typeof(BigInteger), typeof(BigInteger) }, null));
            _bigInteger_op_LessThan = _mod.Import(typeof(BigInteger).GetMethod("op_LessThan", _publicStatic, null, new[] { typeof(BigInteger), typeof(BigInteger) }, null));
            _bigInteger_op_Equality = _mod.Import(typeof(BigInteger).GetMethod("op_Equality", _publicStatic, null, new[] { typeof(BigInteger), typeof(BigInteger) }, null));
            _bigInteger_op_OnesComplement = _mod.Import(typeof(BigInteger).GetMethod("op_OnesComplement", _publicStatic, null, new[] { typeof(BigInteger) }, null));
            _char_IsSurrogate = _mod.Import(typeof(char).GetMethod("IsSurrogate", _publicStatic, null, new[] { typeof(string), typeof(int) }, null));
            _string_get_Chars = _mod.Import(typeof(string).GetMethod("get_Chars", _publicInstance, null, new[] { typeof(int) }, null));
            _bigInteger_get_IsZero = _mod.Import(typeof(BigInteger).GetMethod("get_IsZero", _publicInstance, null, Type.EmptyTypes, null));

            _bigInteger_op_Explicit_toInt = _mod.Import(typeof(BigInteger).GetMethods(_publicStatic).Single(m => m.Name == "op_Explicit" && m.ReturnType == typeof(int)));

            _functionTypes = new Dictionary<FuncitonFunction, FunctionTypeInfo>();
            _nodeInfos = new Dictionary<FuncitonFunction.Node, NodeInfo>();
            _callNodeFields = new Dictionary<FuncitonFunction.CallNode, CallNodeInfo>();
            _inputFields = new Dictionary<FuncitonFunction.InputNode, FieldDefinition>();

            _interface = new TypeDefinition(null, "i", TypeAttributes.Abstract | TypeAttributes.AutoClass | TypeAttributes.AutoLayout | TypeAttributes.Interface | TypeAttributes.NotPublic);
            _mod.Types.Add(_interface);
            _interfaceMethod = new MethodDefinition("i", MethodAttributes.Abstract | MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual, _bigInteger);
            _interface.Methods.Add(_interfaceMethod);

            // STEP 1: Create a type for every function
            // (and within each type, fields and methods for multi-use nodes)
            CreateTypeForFunctionAndRecurse(program);

            // STEP 2: Generate IL code for each method
            foreach (var kvp in _nodeInfos)
                if (kvp.Value.Method != null)
                {
                    var instr = kvp.Value.Method.Body.Instructions;
                    GenerateIL(instr, kvp.Key, true);
                    instr.Add(Instruction.Create(OpCodes.Ret));
                }

            // Finally, create the entry point method (which evaluates the Funciton program and then converts the result to a string and outputs it)
            var entryPointMethod = new MethodDefinition("Main", MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, _void);
            entryPointMethod.Body.InitLocals = true;
            _functionTypes[program].Type.Methods.Add(entryPointMethod);
            _mod.EntryPoint = entryPointMethod;

            var sb = new VariableDefinition(_mod.Import(typeof(StringBuilder)));
            var t = new VariableDefinition(_string);
            entryPointMethod.Body.Variables.Add(sb);
            entryPointMethod.Body.Variables.Add(t);

            convertToInstructions(entryPointMethod.Body.Instructions, newArray(
                Instruction.Create(OpCodes.Call, _nodeInfos[program.OutputNodes.Single(ou => ou != null)].Method),
                Instruction.Create(OpCodes.Newobj, _mod.Import(typeof(StringBuilder).GetConstructor(Type.EmptyTypes))),
                Instruction.Create(OpCodes.Stloc, sb),
                Tuple.Create("LOOP", Instruction.Create(OpCodes.Dup)),
                Instruction.Create(OpCodes.Call, _bigInteger_get_Zero),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Equality),
                Tuple.Create(OpCodes.Brtrue, "DONE"),
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Equality),
                Tuple.Create(OpCodes.Brtrue, "DONE"),
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Ldc_I4, (1 << 21) - 1),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_int),
                Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseAnd),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Explicit_toInt),
                Instruction.Create(OpCodes.Call, _mod.Import(typeof(char).GetMethod("ConvertFromUtf32", _publicStatic, null, new[] { typeof(int) }, null))),
                Instruction.Create(OpCodes.Stloc, t),
                Instruction.Create(OpCodes.Ldloc, sb),
                Instruction.Create(OpCodes.Ldloc, t),
                Instruction.Create(OpCodes.Callvirt, _mod.Import(typeof(StringBuilder).GetMethod("Append", _publicInstance, null, new[] { typeof(string) }, null))),
                Instruction.Create(OpCodes.Pop),
                Instruction.Create(OpCodes.Ldc_I4, 21),
                Instruction.Create(OpCodes.Call, _bigInteger_op_RightShift),
                Tuple.Create(OpCodes.Br, "LOOP"),
                Tuple.Create("DONE", Instruction.Create(OpCodes.Pop)),
                Instruction.Create(OpCodes.Ldloc, sb),
                Instruction.Create(OpCodes.Callvirt, _mod.Import(typeof(object).GetMethod("ToString", _publicInstance, null, Type.EmptyTypes, null))),
                Instruction.Create(OpCodes.Call, _mod.Import(typeof(Console).GetMethod("Write", _publicStatic, null, new[] { typeof(string) }, null))),
                Instruction.Create(OpCodes.Ret)
            ));
        }

        private object[] newArray(params object[] array) { return array; }

        private MethodDefinition GetStdinMethod()
        {
            if (_stdinMethod == null)
            {
                var type = new TypeDefinition(null, "♦", TypeAttributes.Abstract | TypeAttributes.AutoClass | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed, _object);
                var booleanField = new FieldDefinition("b", FieldAttributes.Private | FieldAttributes.Static, _bool);
                var valueField = new FieldDefinition("v", FieldAttributes.Private | FieldAttributes.Static, _bigInteger);
                _stdinMethod = new MethodDefinition("m", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static, _bigInteger);
                _stdinMethod.Body.InitLocals = true;
                type.Fields.Add(booleanField);
                type.Fields.Add(valueField);
                type.Methods.Add(_stdinMethod);
                _mod.Types.Add(type);

                var instr = _stdinMethod.Body.Instructions;
                var a = new VariableDefinition(_int);
                var i = new VariableDefinition(_int);
                var s = new VariableDefinition(_string);
                _stdinMethod.Body.Variables.Add(a);
                _stdinMethod.Body.Variables.Add(i);
                _stdinMethod.Body.Variables.Add(s);

                convertToInstructions(instr, newArray(
                    Instruction.Create(OpCodes.Ldsfld, booleanField),
                    Tuple.Create(OpCodes.Brfalse, "MUSTCOMPUTE"),
                    Instruction.Create(OpCodes.Ldsfld, valueField),
                    Instruction.Create(OpCodes.Ret),
                    Tuple.Create("MUSTCOMPUTE", Instruction.Create(OpCodes.Ldc_I4_1)),
                    Instruction.Create(OpCodes.Stsfld, booleanField),
                    Instruction.Create(OpCodes.Call, _bigInteger_get_Zero),
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Stloc, a),
                    Instruction.Create(OpCodes.Stloc, i),
                    Instruction.Create(OpCodes.Call, _encoding_get_UTF8),
                    Instruction.Create(OpCodes.Call, _console_set_InputEncoding),
                    Instruction.Create(OpCodes.Call, _console_get_In),
                    Instruction.Create(OpCodes.Callvirt, _textReader_ReadToEnd),
                    Instruction.Create(OpCodes.Stloc, s),
                    Instruction.Create(OpCodes.Ldloc, s),
                    Instruction.Create(OpCodes.Callvirt, _string_get_Length),
                    Tuple.Create(OpCodes.Brtrue, "LOOPSTART"),
                    Tuple.Create(OpCodes.Br, "DONE"),
                    Tuple.Create("LOOPSTART", Instruction.Create(OpCodes.Ldloc, i)),
                    Instruction.Create(OpCodes.Ldloc, s),
                    Instruction.Create(OpCodes.Callvirt, _string_get_Length),
                    Tuple.Create(OpCodes.Bge, "LOOPEND"),
                    Instruction.Create(OpCodes.Ldloc, s),
                    Instruction.Create(OpCodes.Ldloc, i),
                    Instruction.Create(OpCodes.Call, _char_ConvertToUtf32),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_int),
                    Instruction.Create(OpCodes.Ldloc, a),
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Ldc_I4, 21),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Stloc, a),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_LeftShift),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseOr),
                    Instruction.Create(OpCodes.Ldloc, s),
                    Instruction.Create(OpCodes.Ldloc, i),
                    Instruction.Create(OpCodes.Call, _char_IsSurrogate),
                    Tuple.Create(OpCodes.Brtrue, "TWO"),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Tuple.Create(OpCodes.Br, "AFTER_TWO"),
                    Tuple.Create("TWO", Instruction.Create(OpCodes.Ldc_I4_2)),
                    Tuple.Create("AFTER_TWO", Instruction.Create(OpCodes.Ldloc, i)),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Stloc, i),
                    Tuple.Create(OpCodes.Br, "LOOPSTART"),
                    Tuple.Create("LOOPEND", Instruction.Create(OpCodes.Ldloc, s)),
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Callvirt, _string_get_Length),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Call, _string_get_Chars),
                    Tuple.Create(OpCodes.Brfalse, "NEGATE"),
                    Tuple.Create(OpCodes.Br, "DONE"),
                    Tuple.Create("NEGATE", Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne)),
                    Instruction.Create(OpCodes.Ldloc, a),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_LeftShift),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseOr),
                    Tuple.Create("DONE", Instruction.Create(OpCodes.Dup)),
                    Instruction.Create(OpCodes.Stsfld, valueField),
                    Instruction.Create(OpCodes.Ret)
                ));
            }
            return _stdinMethod;
        }

        private static void convertToInstructions(Collection<Instruction> instr, object[] data)
        {
            foreach (var obj in data)
            {
                if (obj is Action)
                    ((Action) obj)();
                else if (obj is Instruction)
                    instr.Add((Instruction) obj);
                else if (obj is Tuple<string, Instruction>)
                    instr.Add(((Tuple<string, Instruction>) obj).Item2);
                else
                {
                    var tup = (Tuple<OpCode, string>) obj;
                    instr.Add(Instruction.Create(tup.Item1, data.OfType<Tuple<string, Instruction>>().First(t => t.Item1 == tup.Item2).Item2));
                }
            }
        }

        private void CreateTypeForFunctionAndRecurse(FuncitonFunction f)
        {
            if (_functionTypes.ContainsKey(f))
                return;

            var type = new TypeDefinition(null,
                f is FuncitonProgram ? "p" : "f" + f.Name,
                (f is FuncitonProgram ? TypeAttributes.Abstract | TypeAttributes.Sealed : 0) | TypeAttributes.AutoClass | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.NotPublic,
                _object);
            _functionTypes[f] = new FunctionTypeInfo { Type = type };
            _mod.Types.Add(type);

            var nodes = f.FindNodes();

            if (!(f is FuncitonProgram))
            {
                var constructor = new MethodDefinition(".ctor", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, _void);
                constructor.Body.InitLocals = true;
                var instr = constructor.Body.Instructions;
                instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                instr.Add(Instruction.Create(OpCodes.Call, _object_ctor));
                foreach (var inp in nodes.AllNodes.OfType<FuncitonFunction.InputNode>().OrderBy(n => n.InputPosition))
                {
                    var name = "↑←↓→"[inp.InputPosition].ToString();
                    var paramDef = new ParameterDefinition(name, 0, _interface);
                    constructor.Parameters.Add(paramDef);
                    var fieldDef = new FieldDefinition(name, FieldAttributes.InitOnly | FieldAttributes.Private, _interface);
                    type.Fields.Add(fieldDef);
                    instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                    instr.Add(Instruction.Create(OpCodes.Ldarg, paramDef));
                    instr.Add(Instruction.Create(OpCodes.Stfld, fieldDef));
                    _inputFields.Add(inp, fieldDef);
                }
                instr.Add(Instruction.Create(OpCodes.Ret));
                type.Methods.Add(constructor);
                _functionTypes[f].Constructor = constructor;
            }

            int i = 0;
            Func<bool, MethodDefinition> createMethod = isPublic =>
            {
                var m = new MethodDefinition("M" + i,
                    MethodAttributes.HideBySig | (isPublic ? MethodAttributes.Public : MethodAttributes.Private) | (f is FuncitonProgram ? MethodAttributes.Static : 0),
                    _bigInteger);
                m.Body.InitLocals = true;
                return m;
            };

            foreach (var node in nodes.AllNodes)
            {
                var nodeInfo = new NodeInfo { IsSingleUse = true };
                _nodeInfos.Add(node, nodeInfo);

                // Need to create a method for it if it’s used as input or output
                var isOutput = f.OutputNodes.Contains(node);
                if (isOutput || nodes.NodesUsedAsFunctionInputs.Contains(node))
                    type.Methods.Add(nodeInfo.Method = createMethod(isOutput));

                // Need to create two fields for it if it’s used multiple times
                if (nodes.MultiUseNodes.Contains(node))
                {
                    type.Fields.Add(nodeInfo.BooleanField = new FieldDefinition("E" + i, FieldAttributes.Private | (f is FuncitonProgram ? FieldAttributes.Static : 0), _bool));
                    type.Fields.Add(nodeInfo.ValueField = new FieldDefinition("V" + i, FieldAttributes.Private | (f is FuncitonProgram ? FieldAttributes.Static : 0), _bigInteger));
                    nodeInfo.IsSingleUse = false;
                }
                i++;
            }

            i = 0;
            foreach (var node in nodes.AllNodes.OfType<FuncitonFunction.CallOutputNode>())
            {
                CreateTypeForFunctionAndRecurse(node.CallNode.Function);
                CallNodeInfo inf;
                if (_callNodeFields.TryGetValue(node.CallNode, out inf))
                    inf.CallOutputNodes.Add(node);
                else
                {
                    var j = i;
                    FieldDefinition field = null;
                    _callNodeFields[node.CallNode] = new CallNodeInfo
                    {
                        CallOutputNodes = new List<FuncitonFunction.CallOutputNode> { node },
                        GetField = () =>
                        {
                            if (field == null)
                            {
                                field = new FieldDefinition("C" + j, FieldAttributes.Private | (f is FuncitonProgram ? FieldAttributes.Static : 0), _functionTypes[node.CallNode.Function].Type);
                                type.Fields.Add(field);
                            }
                            return field;
                        }
                    };
                    i++;
                }
            }
        }

        private void GenerateIL(Collection<Instruction> instr, FuncitonFunction.Node node, bool skipDic = false)
        {
            var inf = _nodeInfos[node];

            if (inf.IsSingleUse)
                GenerateILInner(instr, node, skipDic);
            else if (inf.BooleanField.IsStatic)
                convertToInstructions(instr, newArray(
                    Instruction.Create(OpCodes.Ldsfld, inf.BooleanField),
                    Tuple.Create(OpCodes.Brtrue, "TRUE"),
                    new Action(() => { GenerateILInner(instr, node, skipDic); }),
                    Instruction.Create(OpCodes.Stsfld, inf.ValueField),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Stsfld, inf.BooleanField),
                    Tuple.Create("TRUE", Instruction.Create(OpCodes.Ldsfld, inf.ValueField))
                ));
            else
                convertToInstructions(instr, newArray(
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldfld, inf.BooleanField),
                    Tuple.Create(OpCodes.Brtrue, "TRUE"),
                    Instruction.Create(OpCodes.Ldarg_0),
                    new Action(() => { GenerateILInner(instr, node, skipDic); }),
                    Instruction.Create(OpCodes.Stfld, inf.ValueField),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Stfld, inf.BooleanField),
                    Tuple.Create("TRUE", Instruction.Create(OpCodes.Ldarg_0)),
                    Instruction.Create(OpCodes.Ldfld, inf.ValueField)
                ));
        }

        private void GenerateILInner(Collection<Instruction> instr, FuncitonFunction.Node node, bool skipDic = false)
        {
            NodeInfo info;
            if (!skipDic && _nodeInfos.TryGetValue(node, out info) && info.Method != null)
            {
                if (info.Method.IsStatic)
                    instr.Add(Instruction.Create(OpCodes.Call, info.Method));
                else
                {
                    instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                    instr.Add(Instruction.Create(OpCodes.Callvirt, info.Method));
                }
                return;
            }

            var b = TryType<FuncitonFunction.CallOutputNode>(node, c =>
            {
                var inf = _callNodeFields[c.CallNode];

                Action invokeConstructor = () =>
                {
                    foreach (var input in c.CallNode.Inputs)
                        if (input != null)
                        {
                            var intf = _nodeInfos[input].InterfaceType;
                            if (intf == null)
                            {
                                var meth = _nodeInfos[input].Method;
                                if (!meth.IsStatic && !meth.DeclaringType.Interfaces.Contains(_interface))
                                {
                                    // If this type doesn’t implement the interface yet, do so now
                                    meth.DeclaringType.Interfaces.Add(_interface);
                                    meth.Overrides.Add(_interfaceMethod);
                                    meth.IsVirtual = true;
                                    meth.IsFinal = true;
                                    _nodeInfos[input].InterfaceType = intf = meth.DeclaringType;
                                }
                                else
                                {
                                    // Otherwise, need to create a new type just to implement the interface
                                    var interfaceImplementor = new TypeDefinition(null, "i" + meth.Name, TypeAttributes.AutoClass | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.Sealed, _object);
                                    meth.DeclaringType.NestedTypes.Add(interfaceImplementor);
                                    interfaceImplementor.Interfaces.Add(_interface);
                                    FieldDefinition field = null;
                                    if (!meth.IsStatic)
                                    {
                                        field = new FieldDefinition("u", FieldAttributes.InitOnly | FieldAttributes.Private, meth.DeclaringType);
                                        interfaceImplementor.Fields.Add(field);
                                    }
                                    var interfaceImplementingMethod = new MethodDefinition(meth.Name, MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Private | MethodAttributes.Virtual, _bigInteger);
                                    interfaceImplementor.Methods.Add(interfaceImplementingMethod);
                                    interfaceImplementingMethod.Overrides.Add(_interfaceMethod);
                                    if (!meth.IsStatic)
                                    {
                                        interfaceImplementingMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        interfaceImplementingMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
                                    }
                                    interfaceImplementingMethod.Body.Instructions.Add(Instruction.Create(meth.IsStatic ? OpCodes.Call : OpCodes.Callvirt, meth));
                                    interfaceImplementingMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                    var constructor = new MethodDefinition(".ctor", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, _void);
                                    interfaceImplementor.Methods.Add(constructor);
                                    if (!meth.IsStatic)
                                        constructor.Parameters.Add(new ParameterDefinition(meth.DeclaringType));
                                    constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, _object_ctor));
                                    if (!meth.IsStatic)
                                    {
                                        constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                                        constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                                        constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
                                    }
                                    constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                    _nodeInfos[input].InterfaceType = intf = interfaceImplementor;
                                    _nodeInfos[input].InterfaceTypeConstructor = constructor;
                                }
                            }

                            if (!_nodeInfos[input].Method.IsStatic)
                                instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                            if (_nodeInfos[input].InterfaceTypeConstructor != null)
                                instr.Add(Instruction.Create(OpCodes.Newobj, _nodeInfos[input].InterfaceTypeConstructor));
                        }

                    instr.Add(Instruction.Create(OpCodes.Newobj, _functionTypes[c.CallNode.Function].Constructor));
                };

                if (inf.CallOutputNodes.Count == 1)
                {
                    // The function has only one output: generate IL that will instantiate the function type, call the one output method on it, and then discard it
                    invokeConstructor();
                    instr.Add(Instruction.Create(OpCodes.Call, _nodeInfos[c.CallNode.Function.OutputNodes[c.OutputPosition]].Method));
                }
                else
                {
                    // The function has multiple outputs: we have to place the function instance in a field in order to ensure that it is instantiated only once
                    var field = inf.GetField();
                    convertToInstructions(instr, field.IsStatic ? newArray(
                        Instruction.Create(OpCodes.Ldsfld, field),
                        Tuple.Create(OpCodes.Brtrue, "TRUE"),
                        invokeConstructor,
                        Instruction.Create(OpCodes.Stsfld, field),
                        Tuple.Create("TRUE", Instruction.Create(OpCodes.Ldsfld, field))
                    ) : newArray(
                        Instruction.Create(OpCodes.Ldarg_0),
                        Instruction.Create(OpCodes.Ldfld, field),
                        Tuple.Create(OpCodes.Brtrue, "TRUE"),
                        Instruction.Create(OpCodes.Ldarg_0),
                        invokeConstructor,
                        Instruction.Create(OpCodes.Stfld, field),
                        Tuple.Create("TRUE", Instruction.Create(OpCodes.Ldarg_0)),
                        Instruction.Create(OpCodes.Ldfld, field)
                    ));
                    instr.Add(Instruction.Create(OpCodes.Call, _nodeInfos[c.CallNode.Function.OutputNodes[c.OutputPosition]].Method));
                }
            }) || TryType<FuncitonFunction.LiteralNode>(node, c =>
            {
                if (c.Result <= int.MaxValue && c.Result >= int.MinValue)
                {
                    instr.Add(Instruction.Create(OpCodes.Ldc_I4, (int) c.Result));
                    instr.Add(Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_int));
                }
                else if (c.Result <= long.MaxValue && c.Result >= long.MinValue)
                {
                    instr.Add(Instruction.Create(OpCodes.Ldc_I8, (long) c.Result));
                    instr.Add(Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_long));
                }
                else
                {
                    instr.Add(Instruction.Create(OpCodes.Ldstr, c.Result.ToString()));
                    instr.Add(Instruction.Create(OpCodes.Call, _bigInteger_Parse));
                }
            }) || TryType<FuncitonFunction.StdInNode>(node, c =>
            {
                instr.Add(Instruction.Create(OpCodes.Call, GetStdinMethod()));
            }) || TryType<FuncitonFunction.NandNode>(node, c =>
            {
                // Optimise NAND with 0
                var literal = c.Left as FuncitonFunction.LiteralNode;
                if (literal != null && literal.Result == 0)
                {
                    instr.Add(Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne));
                    return;
                }

                GenerateIL(instr, c.Left);

                // Optimise bitwise-not
                if (c.Left == c.Right)
                {
                    instr.Add(Instruction.Create(OpCodes.Call, _bigInteger_op_OnesComplement));
                    return;
                }

                convertToInstructions(instr, newArray(
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Call, _bigInteger_get_Zero),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_Equality),
                    Tuple.Create(OpCodes.Brtrue, "DONE"),
                    new Action(() => { GenerateIL(instr, c.Right); }),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseAnd),
                    Tuple.Create("DONE", Instruction.Create(OpCodes.Call, _bigInteger_op_OnesComplement))
                ));
            }) || TryType<FuncitonFunction.InputNode>(node, c =>
            {
                instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                instr.Add(Instruction.Create(OpCodes.Ldfld, _inputFields[c]));
                instr.Add(Instruction.Create(OpCodes.Callvirt, _interfaceMethod));
            }) || TryType<FuncitonFunction.LessThanNode>(node, c =>
            {
                GenerateIL(instr, c.Left);
                GenerateIL(instr, c.Right);
                convertToInstructions(instr, newArray(
                    Instruction.Create(OpCodes.Call, _bigInteger_op_LessThan),
                    Tuple.Create(OpCodes.Brtrue, "MINUSONE"),
                    Instruction.Create(OpCodes.Call, _bigInteger_get_Zero),
                    Tuple.Create(OpCodes.Br, "DONE"),
                    Tuple.Create("MINUSONE", Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne)),
                    Tuple.Create("DONE", Instruction.Create(OpCodes.Nop))
                ));
            }) || TryType<FuncitonFunction.ShiftLeftNode>(node, c =>
            {
                GenerateIL(instr, c.Left);
                GenerateIL(instr, c.Right);
                convertToInstructions(instr, newArray(
                    Instruction.Create(OpCodes.Call, _bigInteger_op_Explicit_toInt),
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Tuple.Create(OpCodes.Blt, "NEGATIVE"),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_LeftShift),
                    Tuple.Create(OpCodes.Br, "DONE"),
                    Tuple.Create("NEGATIVE", Instruction.Create(OpCodes.Neg)),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_RightShift),
                    Tuple.Create("DONE", Instruction.Create(OpCodes.Nop))
                ));
            });

            if (!b)
            {
                System.Diagnostics.Debugger.Break();
                throw new InvalidOperationException();
            }
        }

        private bool TryType<T>(object obj, Action<T> action)
        {
            if (obj is T)
            {
                action((T) obj);
                return true;
            }
            return false;
        }

        private AssemblyDefinition Assembly { get { return _asm; } }

        private sealed class FunctionTypeInfo
        {
            public TypeDefinition Type;
            public MethodDefinition Constructor;
        }

        private sealed class NodeInfo
        {
            public FieldDefinition BooleanField;
            public FieldDefinition ValueField;
            public MethodDefinition Method;
            public TypeDefinition InterfaceType;
            public MethodDefinition InterfaceTypeConstructor;
            public bool IsSingleUse;
        }

        private sealed class CallNodeInfo
        {
            public List<FuncitonFunction.CallOutputNode> CallOutputNodes;
            public Func<FieldDefinition> GetField;
        }
    }
}
