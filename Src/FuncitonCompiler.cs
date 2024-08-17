using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Funciton
{
    class FuncitonCompiler
    {
        public static void CompileTo(FuncitonProgram program, string targetFilePath)
        {
            new FuncitonCompiler(program, Path.GetFileNameWithoutExtension(targetFilePath))._asm.Write(targetFilePath);
        }

        private readonly AssemblyDefinition _asm;
        private readonly ModuleDefinition _mod;
        private MethodDefinition _stdinMethod;
        private readonly FieldDefinition _result;
        private readonly FieldDefinition _lambdaList;

        private readonly TypeDefinition _delegate;
        private readonly MethodDefinition _delegate_ctor;
        private readonly MethodDefinition _delegateInvoke;

        private readonly TypeDefinition _closureDelegate;
        private readonly MethodDefinition _closureDelegate_ctor;
        private readonly MethodDefinition _closureDelegateInvoke;

        private readonly TypeReference _void;
        private readonly TypeReference _bool;
        private readonly TypeReference _int;

        private readonly TypeReference _lambdaListType;
        private readonly MethodReference _lambdaList_ctor;
        private readonly MethodReference _lambdaListAdd;
        private readonly MethodReference _lambdaListCount;
        private readonly MethodReference _lambdaListGetItem;

        private readonly TypeReference _delegateTuple;
        private readonly MethodReference _delegateTuple_ctor;
        private readonly MethodReference _delegateTupleGetItem1;
        private readonly MethodReference _delegateTupleGetItem2;

        private readonly TypeReference _object;
        private readonly MethodReference _object_ctor;

        private readonly TypeReference _string;
        private readonly MethodReference _string_get_Length;
        private readonly MethodReference _string_get_Chars;

        private readonly TypeReference _stack;
        private readonly MethodReference _stack_ctor;
        private readonly MethodReference _stack_Push;
        private readonly MethodReference _stack_Pop;
        private readonly MethodReference _stack_get_Count;

        private readonly TypeReference _bigInteger;
        private readonly MethodReference _bigInteger_Parse;
        private readonly MethodReference _bigInteger_get_MinusOne;
        private readonly MethodReference _bigInteger_get_Zero;
        private readonly MethodReference _bigInteger_op_Implicit_int;
        private readonly MethodReference _bigInteger_op_Implicit_long;
        private readonly MethodReference _bigInteger_op_LeftShift;
        private readonly MethodReference _bigInteger_op_RightShift;
        private readonly MethodReference _bigInteger_op_BitwiseOr;
        private readonly MethodReference _bigInteger_op_BitwiseAnd;
        private readonly MethodReference _bigInteger_op_OnesComplement;
        private readonly MethodReference _bigInteger_op_LessThan;
        private readonly MethodReference _bigInteger_op_Equality;
        private readonly MethodReference _bigInteger_op_Explicit_toInt;

        private readonly MethodReference _char_ConvertToUtf32;
        private readonly MethodReference _char_IsSurrogate;
        private readonly MethodReference _console_set_InputEncoding;
        private readonly MethodReference _console_get_In;
        private readonly MethodReference _encoding_get_UTF8;
        private readonly MethodReference _textReader_ReadToEnd;

        private readonly Dictionary<FuncitonFunction, FunctionTypeInfo> _functionTypes;
        private readonly Dictionary<Node, NodeInfo> _nodeInfos;
        private readonly Dictionary<Call, CallInfo> _callInfos;
        private readonly Dictionary<LambdaInvocation, LambdaInvocationInfo> _lambdaInvocationInfos;
        private readonly Dictionary<InputNode, FieldDefinition> _inputFields;

        private int _branchCount = 0;

        private MethodReference getMethod<T>(Expression<Action<T>> expr) => getMethod(expr.Body);
        private MethodReference getMethod<TResult>(Expression<Func<TResult>> expr) => getMethod(expr.Body);
        private MethodReference getMethod<T, TResult>(Expression<Func<T, TResult>> expr) => getMethod(expr.Body);
        private MethodReference getMethod<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expr) => getMethod(expr.Body);
        private MethodReference getMethod(Expression expr) => expr switch
        {
            MethodCallExpression meth => _mod.Import(meth.Method),
            MemberExpression mem => _mod.Import(((System.Reflection.PropertyInfo) mem.Member).GetGetMethod(true)),
            BinaryExpression bin => _mod.Import(bin.Method),
            UnaryExpression un => _mod.Import(un.Method),
            _ => throw new InternalErrorException($"Unexpected type of expression: {expr.GetType().FullName}")
        };
        private MethodReference getSetter<TResult>(Expression<Func<TResult>> expr) =>
            _mod.Import(((System.Reflection.PropertyInfo) ((MemberExpression) expr.Body).Member).GetSetMethod(true));

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

            _bigInteger_op_Implicit_long = getMethod((long b) => (BigInteger) b);
            _bigInteger_Parse = getMethod((string s) => BigInteger.Parse(s));
            _bigInteger_get_Zero = getMethod(() => BigInteger.Zero);
            _bigInteger_get_MinusOne = getMethod(() => BigInteger.MinusOne);
            _encoding_get_UTF8 = getMethod(() => Encoding.UTF8);
            _console_set_InputEncoding = getSetter(() => Console.InputEncoding);
            _console_get_In = getMethod(() => Console.In);
            _textReader_ReadToEnd = getMethod((TextReader x) => x.ReadToEnd());
            _string_get_Length = getMethod((string x) => x.Length);
            _char_ConvertToUtf32 = getMethod((string x, int y) => char.ConvertToUtf32(x, y));
            _bigInteger_op_Implicit_int = getMethod((int x) => (BigInteger) x);
            _bigInteger_op_LeftShift = getMethod((BigInteger x, int y) => x << y);
            _bigInteger_op_RightShift = getMethod((BigInteger x, int y) => x >> y);
            _bigInteger_op_BitwiseOr = getMethod((BigInteger x, BigInteger y) => x | y);
            _bigInteger_op_BitwiseAnd = getMethod((BigInteger x, BigInteger y) => x & y);
            _bigInteger_op_LessThan = getMethod((BigInteger x, BigInteger y) => x < y);
            _bigInteger_op_Equality = getMethod((BigInteger x, BigInteger y) => x == y);
            _bigInteger_op_OnesComplement = getMethod((BigInteger x) => ~x);
            _char_IsSurrogate = getMethod((string x, int y) => char.IsSurrogate(x, y));
            _string_get_Chars = getMethod((string x, int y) => x[y]);
            _bigInteger_op_Explicit_toInt = getMethod((BigInteger x) => (int) x);

            _functionTypes = [];
            _nodeInfos = [];
            _callInfos = [];
            _lambdaInvocationInfos = [];
            _inputFields = [];

            _delegate = new TypeDefinition(null, "➲", TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed, _mod.Import(typeof(MulticastDelegate)));
            _delegate_ctor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, _void);
            _delegate_ctor.IsRuntime = true;
            _delegate_ctor.Parameters.Add(new ParameterDefinition(_object));
            _delegate_ctor.Parameters.Add(new ParameterDefinition(_mod.Import(typeof(IntPtr))));
            _delegateInvoke = new MethodDefinition("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, _delegate);
            _delegateInvoke.IsRuntime = true;
            _delegate.Methods.Add(_delegate_ctor);
            _delegate.Methods.Add(_delegateInvoke);
            _mod.Types.Add(_delegate);

            var tupleGeneric = _mod.Import(typeof(Tuple<,>));
            _delegateTuple = tupleGeneric.MakeGenericInstanceType(_delegate, _delegate);
            _delegateTuple_ctor = new MethodReference(".ctor", _void, _delegateTuple) { HasThis = true };
            _delegateTuple_ctor.Parameters.Add(new ParameterDefinition(tupleGeneric.GenericParameters[0]));
            _delegateTuple_ctor.Parameters.Add(new ParameterDefinition(tupleGeneric.GenericParameters[1]));
            _delegateTupleGetItem1 = new MethodReference("get_Item1", tupleGeneric.GenericParameters[0], _delegateTuple) { HasThis = true };
            _delegateTupleGetItem2 = new MethodReference("get_Item2", tupleGeneric.GenericParameters[1], _delegateTuple) { HasThis = true };

            _closureDelegate = new TypeDefinition(null, "☂", TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed, _mod.Import(typeof(MulticastDelegate)));
            _closureDelegate_ctor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, _void);
            _closureDelegate_ctor.IsRuntime = true;
            _closureDelegate_ctor.Parameters.Add(new ParameterDefinition(_object));
            _closureDelegate_ctor.Parameters.Add(new ParameterDefinition(_mod.Import(typeof(IntPtr))));
            _closureDelegateInvoke = new MethodDefinition("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, _delegateTuple);
            _closureDelegateInvoke.Parameters.Add(new ParameterDefinition(_delegate));
            _closureDelegateInvoke.IsRuntime = true;
            _closureDelegate.Methods.Add(_closureDelegate_ctor);
            _closureDelegate.Methods.Add(_closureDelegateInvoke);
            _mod.Types.Add(_closureDelegate);

            var stackGeneric = _mod.Import(typeof(Stack<>));
            _stack = stackGeneric.MakeGenericInstanceType(_delegate);
            _stack_ctor = new MethodReference(".ctor", _void, _stack) { HasThis = true };
            _stack_ctor.Parameters.Add(new ParameterDefinition(_int));
            _stack_Push = new MethodReference("Push", _void, _stack) { HasThis = true };
            _stack_Push.Parameters.Add(new ParameterDefinition(stackGeneric.GenericParameters[0]));
            _stack_Pop = new MethodReference("Pop", stackGeneric.GenericParameters[0], _stack) { HasThis = true };
            _stack_get_Count = new MethodReference("get_Count", _int, _stack) { HasThis = true };

            var lambdaListGeneric = _mod.Import(typeof(List<>));
            _lambdaListType = lambdaListGeneric.MakeGenericInstanceType(_closureDelegate);
            _lambdaList_ctor = new MethodReference(".ctor", _void, _lambdaListType) { HasThis = true };
            _lambdaList_ctor.Parameters.Add(new ParameterDefinition(_int));
            _lambdaListAdd = new MethodReference("Add", _void, _lambdaListType) { HasThis = true };
            _lambdaListAdd.Parameters.Add(new ParameterDefinition(lambdaListGeneric.GenericParameters[0]));
            _lambdaListCount = new MethodReference("get_Count", _int, _lambdaListType) { HasThis = true };
            _lambdaListGetItem = new MethodReference("get_Item", lambdaListGeneric.GenericParameters[0], _lambdaListType) { HasThis = true };
            _lambdaListGetItem.Parameters.Add(new ParameterDefinition(_int));

            _result = new FieldDefinition("⏎", FieldAttributes.Assembly | FieldAttributes.Static, _bigInteger);
            _lambdaList = new FieldDefinition("☣", FieldAttributes.Assembly | FieldAttributes.Static, _lambdaListType);

            // Create a type for every function
            CreateTypeForFunctionAndRecurse(program);
            _functionTypes[program].Type.Fields.Add(_result);
            _functionTypes[program].Type.Fields.Add(_lambdaList);

            // Generate IL code for each method
            foreach (var kvp in _nodeInfos)
            {
                if (kvp.Value.Method == null)
                    continue;

                convertToInstructions(
                    kvp.Value.Method,
                    kvp.Value.CreateTemporaryLocal,
                    false,
                    GenerateIL(kvp.Key, kvp.Value.CreateTemporaryLocal, 0, true).ToArray()
                );
            }

            // Fill in the IL for the copy constructors (they were previously created by CreateTypeForFunctionAndRecurse())
            foreach (var inf in _functionTypes.Values)
            {
                if (inf.CopyConstructor == null)
                    continue;
                var instr = inf.CopyConstructor.Body.Instructions;
                instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                instr.Add(Instruction.Create(OpCodes.Call, _object_ctor));
                foreach (var field in inf.Type.Fields)
                {
                    if (field.IsStatic)
                        continue;
                    instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                    instr.Add(Instruction.Create(OpCodes.Ldarg_1));
                    instr.Add(Instruction.Create(OpCodes.Ldfld, field));
                    instr.Add(Instruction.Create(OpCodes.Stfld, field));
                }
                instr.Add(Instruction.Create(OpCodes.Ret));
            }

            // Finally, create the entry point method (which evaluates the Funciton program and then converts the result to a string and outputs it)
            var entryPointMethod = new MethodDefinition("➠", MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, _void);
            entryPointMethod.Body.InitLocals = true;
            _functionTypes[program].Type.Methods.Add(entryPointMethod);
            _mod.EntryPoint = entryPointMethod;

            // For the stack handling
            var stack = new VariableDefinition(_stack);
            var currentNode = new VariableDefinition(_delegate);
            entryPointMethod.Body.Variables.Add(stack);
            entryPointMethod.Body.Variables.Add(currentNode);

            // For the BigInteger-to-string conversion at the end
            var stringBuilder = new VariableDefinition(_mod.Import(typeof(StringBuilder)));
            var tempStr = new VariableDefinition(_string);
            entryPointMethod.Body.Variables.Add(stringBuilder);
            entryPointMethod.Body.Variables.Add(tempStr);

            convertToInstructions(entryPointMethod,
                type => throw new InvalidOperationException(),
                true,

                // _lambdaList = new List<_closureDelegate>(1024)
                Instruction.Create(OpCodes.Ldc_I4, 1024),
                Instruction.Create(OpCodes.Newobj, _lambdaList_ctor),
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Callvirt, _lambdaListAdd),
                Instruction.Create(OpCodes.Stsfld, _lambdaList),

                // var stack = new Stack<_delegate>(1024)
                Instruction.Create(OpCodes.Ldc_I4, 1024),
                Instruction.Create(OpCodes.Newobj, _stack_ctor),
                Instruction.Create(OpCodes.Stloc, stack),

                // var currentNode = entryMethod
                Instruction.Create(OpCodes.Newobj, _functionTypes[program].Constructor),
                Instruction.Create(OpCodes.Ldftn, _nodeInfos[program.OutputNodes.Single(ou => ou != null)].Method),
                Instruction.Create(OpCodes.Newobj, _delegate_ctor),
                Instruction.Create(OpCodes.Stloc, currentNode),

                // while (true)
                "EVALUATION_LOOP",

                // next = currentNode()
                Instruction.Create(OpCodes.Ldloc, currentNode),
                Instruction.Create(OpCodes.Callvirt, _delegateInvoke),

                // if (next != null)
                Instruction.Create(OpCodes.Dup),
                Tuple.Create(OpCodes.Brfalse, "EVALUATION_NULL"),

                // evaluationStack.Push(currentNode);
                Instruction.Create(OpCodes.Ldloc, stack),
                Instruction.Create(OpCodes.Ldloc, currentNode),
                Instruction.Create(OpCodes.Callvirt, _stack_Push),

                // currentNode = next
                Instruction.Create(OpCodes.Stloc, currentNode),

                // else
                Tuple.Create(OpCodes.Br, "EVALUATION_LOOP"),
                "EVALUATION_NULL",
                Instruction.Create(OpCodes.Pop),

                // if (stack.Count == 0)
                Instruction.Create(OpCodes.Ldloc, stack),
                Instruction.Create(OpCodes.Callvirt, _stack_get_Count),
                Tuple.Create(OpCodes.Brtrue, "EVALUATION_CONTINUE"),

                // convert integer to string, output and exit
                Instruction.Create(OpCodes.Newobj, _mod.Import(typeof(StringBuilder).GetConstructor(Type.EmptyTypes))),
                Instruction.Create(OpCodes.Stloc, stringBuilder),
                Instruction.Create(OpCodes.Ldsfld, _result),
                "CONVERSION_LOOP",
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Call, _bigInteger_get_Zero),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Equality),
                Tuple.Create(OpCodes.Brtrue, "CONVERSION_DONE"),
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Equality),
                Tuple.Create(OpCodes.Brtrue, "CONVERSION_DONE"),
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Ldc_I4, (1 << 21) - 1),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_int),
                Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseAnd),
                Instruction.Create(OpCodes.Call, _bigInteger_op_Explicit_toInt),
                Instruction.Create(OpCodes.Call, getMethod((int x) => char.ConvertFromUtf32(x))),
                Instruction.Create(OpCodes.Stloc, tempStr),
                Instruction.Create(OpCodes.Ldloc, stringBuilder),
                Instruction.Create(OpCodes.Ldloc, tempStr),
                Instruction.Create(OpCodes.Callvirt, getMethod((StringBuilder sb, string s) => sb.Append(s))),
                Instruction.Create(OpCodes.Pop),
                Instruction.Create(OpCodes.Ldc_I4, 21),
                Instruction.Create(OpCodes.Call, _bigInteger_op_RightShift),
                Tuple.Create(OpCodes.Br, "CONVERSION_LOOP"),
                "CONVERSION_DONE",
                Instruction.Create(OpCodes.Pop),
                Instruction.Create(OpCodes.Ldloc, stringBuilder),
                Instruction.Create(OpCodes.Callvirt, getMethod((object o) => o.ToString())),
                Instruction.Create(OpCodes.Call, getMethod((string s) => Console.Write(s))),
                Instruction.Create(OpCodes.Ret),

                "EVALUATION_CONTINUE",
                // currentNode = stack.Pop();
                Instruction.Create(OpCodes.Ldloc, stack),
                Instruction.Create(OpCodes.Callvirt, _stack_Pop),
                Instruction.Create(OpCodes.Stloc, currentNode),
                Tuple.Create(OpCodes.Br, "EVALUATION_LOOP")
            );
        }

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

                var a = new VariableDefinition(_int);
                var i = new VariableDefinition(_int);
                var s = new VariableDefinition(_string);
                _stdinMethod.Body.Variables.Add(a);
                _stdinMethod.Body.Variables.Add(i);
                _stdinMethod.Body.Variables.Add(s);

                convertToInstructions(_stdinMethod,
                    typeDef => throw new InvalidOperationException(),
                    true,

                    Instruction.Create(OpCodes.Ldsfld, booleanField),
                    Tuple.Create(OpCodes.Brfalse, "MUSTCOMPUTE"),
                    Instruction.Create(OpCodes.Ldsfld, valueField),
                    Instruction.Create(OpCodes.Ret),
                    "MUSTCOMPUTE",
                    Instruction.Create(OpCodes.Ldc_I4_1),
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
                    Tuple.Create(OpCodes.Brfalse, "DONE"),
                    "LOOPSTART",
                    Instruction.Create(OpCodes.Ldloc, i),
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
                    "TWO",
                    Instruction.Create(OpCodes.Ldc_I4_2),
                    "AFTER_TWO",
                    Instruction.Create(OpCodes.Ldloc, i),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Stloc, i),
                    Tuple.Create(OpCodes.Br, "LOOPSTART"),
                    "LOOPEND",
                    Instruction.Create(OpCodes.Ldloc, s),
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Callvirt, _string_get_Length),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Call, _string_get_Chars),
                    Tuple.Create(OpCodes.Brtrue, "DONE"),
                    Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne),
                    Instruction.Create(OpCodes.Ldloc, a),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_LeftShift),
                    Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseOr),
                    "DONE",
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Stsfld, valueField),
                    Instruction.Create(OpCodes.Ret)
                );
            }
            return _stdinMethod;
        }

        private void convertToInstructions(MethodDefinition intoMethod, Func<TypeReference, VariableDefinition> getTempVariable, bool isSelfContained, params object[] data)
        {
            var instrs = intoMethod.Body.Instructions;
            FieldDefinition stateField = null;
            FieldDefinition resultField = null;
            VariableDefinition delegateTemp = null;
            List<Instruction> switchTargets = null;
            List<FieldDefinition> bigIntFields = null;
            List<Action<Instruction>> setLastInstruction = [];
            int state = 0;
            foreach (var obj in data)
            {
                if (obj is string)
                    continue;
                else if (obj is Instruction instr)
                    instrs.Add(instr);
                else if (obj is Tuple<OpCode, string> tup)
                {
                    var targetInstructionIndex = Array.IndexOf(data, tup.Item2);
                    while (targetInstructionIndex < data.Length && data[targetInstructionIndex] is string)
                        targetInstructionIndex++;
                    if (targetInstructionIndex == data.Length)
                    {
                        int instrIndex = instrs.Count;
                        instrs.Add(Instruction.Create(OpCodes.Nop));
                        setLastInstruction.Add(lastInstr => { instrs[instrIndex] = Instruction.Create(tup.Item1, lastInstr); });
                    }
                    else
                        instrs.Add(Instruction.Create(tup.Item1, (Instruction) data[targetInstructionIndex]));
                }
                else if (obj is int depth)
                {
                    if (stateField == null)
                    {
                        intoMethod.DeclaringType.Fields.Add(stateField = new FieldDefinition(intoMethod.Name + "⌘", FieldAttributes.Private, _int));
                        intoMethod.DeclaringType.Fields.Add(resultField = new FieldDefinition(intoMethod.Name + "⏎", FieldAttributes.Private, _bigInteger));
                        switchTargets = [instrs[0]];
                        delegateTemp = new VariableDefinition(_delegate);
                        intoMethod.Body.Variables.Add(delegateTemp);
                        bigIntFields = [];
                    }
                    state++;
                    instrs.Add(Instruction.Create(OpCodes.Stloc, delegateTemp));
                    for (int i = 0; i < depth; i++)
                    {
                        if (i == bigIntFields.Count)
                        {
                            var bigIntField = new FieldDefinition(intoMethod.Name + string.Join("", i.ToString().Select(c => (char) (0x2080 + c - '0'))), FieldAttributes.Private, _bigInteger);
                            bigIntFields.Add(bigIntField);
                            intoMethod.DeclaringType.Fields.Add(bigIntField);
                        }
                        var tempVariable = getTempVariable(_bigInteger);
                        instrs.Add(Instruction.Create(OpCodes.Stloc, tempVariable));
                        instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instrs.Add(Instruction.Create(OpCodes.Ldloc, tempVariable));
                        instrs.Add(Instruction.Create(OpCodes.Stfld, bigIntFields[i]));
                    }
                    instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
                    instrs.Add(Instruction.Create(OpCodes.Ldc_I4, state));
                    instrs.Add(Instruction.Create(OpCodes.Stfld, stateField));
                    instrs.Add(Instruction.Create(OpCodes.Ldloc, delegateTemp));
                    instrs.Add(Instruction.Create(OpCodes.Ret));
                    var index = instrs.Count;
                    for (int i = depth - 1; i >= 0; i--)
                    {
                        instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instrs.Add(Instruction.Create(OpCodes.Ldfld, bigIntFields[i]));
                    }
                    instrs.Add(Instruction.Create(OpCodes.Ldsfld, _result));
                    switchTargets.Add(instrs[index]);
                }
                else
                {
                    System.Diagnostics.Debugger.Break();
                    throw new InvalidOperationException();
                }
            }

            var lastInstructionIndex = instrs.Count;
            var extraInstructions = new List<Instruction>();

            if (stateField != null)
            {
                if (isSelfContained)
                    throw new InvalidOperationException("A self-contained method cannot have state changes.");

                // At the end of the method, remember the result, and then Insert the last state, which simply returns that result
                state++;
                var tempVariable = getTempVariable(_bigInteger);
                instrs.Add(Instruction.Create(OpCodes.Stloc, tempVariable));
                instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
                instrs.Add(Instruction.Create(OpCodes.Ldloc, tempVariable));
                instrs.Add(Instruction.Create(OpCodes.Stfld, resultField));
                instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
                instrs.Add(Instruction.Create(OpCodes.Ldc_I4, state));
                instrs.Add(Instruction.Create(OpCodes.Stfld, stateField));
                var lastStateIndex = instrs.Count;

                instrs.Add(Instruction.Create(OpCodes.Ldarg_0));
                instrs.Add(Instruction.Create(OpCodes.Ldfld, resultField));
                instrs.Add(Instruction.Create(OpCodes.Stsfld, _result));
                instrs.Add(Instruction.Create(OpCodes.Ldnull));
                instrs.Add(Instruction.Create(OpCodes.Ret));
                switchTargets.Add(instrs[lastStateIndex]);

                // Insert the switch statement at the beginning of the method
                extraInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                extraInstructions.Add(Instruction.Create(OpCodes.Ldfld, stateField));
                extraInstructions.Add(Instruction.Create(OpCodes.Switch, switchTargets.ToArray()));
            }
            else if (!isSelfContained)
            {
                instrs.Add(Instruction.Create(OpCodes.Stsfld, _result));
                instrs.Add(Instruction.Create(OpCodes.Ldnull));
                instrs.Add(Instruction.Create(OpCodes.Ret));
            }

            foreach (var action in setLastInstruction)
                action(instrs[lastInstructionIndex]);
            for (int i = extraInstructions.Count - 1; i >= 0; i--)
                instrs.Insert(0, extraInstructions[i]);
        }

        private void CreateTypeForFunctionAndRecurse(FuncitonFunction f)
        {
            if (_functionTypes.ContainsKey(f))
                return;

            var type = new TypeDefinition(
                f is FuncitonProgram ? null : "ƒ",
                f is FuncitonProgram ? "➠" : f.Name,
                TypeAttributes.AutoClass | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.NotPublic,
                _object);
            _functionTypes[f] = new FunctionTypeInfo { Type = type };
            _mod.Types.Add(type);

            var nodes = f.FindNodes();

            // Generate the constructor for this function, which takes one parameter for each input to the function (zero for the main program)
            var constructor = new MethodDefinition(".ctor", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, _void);
            constructor.Body.InitLocals = true;
            var instr = constructor.Body.Instructions;
            instr.Add(Instruction.Create(OpCodes.Ldarg_0));
            instr.Add(Instruction.Create(OpCodes.Call, _object_ctor));
            foreach (var inp in nodes.AllNodes.OfType<InputNode>().OrderBy(n => n.InputPosition))
            {
                var name = "↑→↓←"[inp.InputPosition].ToString();
                var paramDef = new ParameterDefinition(name, 0, _delegate);
                constructor.Parameters.Add(paramDef);
                var fieldDef = new FieldDefinition(name, FieldAttributes.Private, _delegate);
                type.Fields.Add(fieldDef);
                instr.Add(Instruction.Create(OpCodes.Ldarg_0));
                instr.Add(Instruction.Create(OpCodes.Ldarg, paramDef));
                instr.Add(Instruction.Create(OpCodes.Stfld, fieldDef));
                _inputFields.Add(inp, fieldDef);
            }
            instr.Add(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(constructor);
            _functionTypes[f].Constructor = constructor;

            {
                var i = 0;
                MethodDefinition createMethod(bool isPublic)
                {
                    var m = new MethodDefinition(i.ToString(),
                        MethodAttributes.HideBySig | (isPublic ? MethodAttributes.Public : MethodAttributes.Private),
                        _delegate);
                    m.Body.InitLocals = true;
                    return m;
                }

                foreach (var node in nodes.AllNodes)
                {
                    var nodeInfo = new NodeInfo();
                    _nodeInfos.Add(node, nodeInfo);

                    // Need to create a method for it if it’s used as input or output
                    var isOutput = f.OutputNodes.Contains(node);
                    if (isOutput || nodes.NodesUsedAsFunctionInputs.Contains(node) || nodes.MultiUseNodes.Contains(node))
                        type.Methods.Add(nodeInfo.Method = createMethod(isOutput));

                    i++;
                }
            }

            // If there are any lambda expression nodes in this function, we need...
            {
                MethodDefinition copyConstructor = null;
                var i = 0;
                foreach (var lambdaExpression in nodes.AllNodes.OfType<LambdaExpressionNode>())
                {
                    // ... a copy constructor (the IL will be filled in later)
                    if (copyConstructor == null)
                    {
                        copyConstructor = new MethodDefinition(".ctor", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, _void);
                        copyConstructor.Body.InitLocals = true;
                        copyConstructor.Parameters.Add(new ParameterDefinition(type));
                        _functionTypes[f].CopyConstructor = copyConstructor;
                        type.Methods.Add(copyConstructor);
                    }

                    // ... for each lambda expression that doesn’t ignore its parameter, a field for the lambda argument
                    FieldDefinition argumentField = null;
                    if (lambdaExpression.Parameter != null)
                    {
                        argumentField = new FieldDefinition("λ" + i, FieldAttributes.Private, _delegate);
                        _nodeInfos[lambdaExpression.Parameter].ArgumentField = argumentField;
                        type.Fields.Add(argumentField);
                    }

                    // ... and a clone method that sets that particular argument
                    var cloneMethod = new MethodDefinition("‼" + i, MethodAttributes.HideBySig | MethodAttributes.Private, _delegateTuple);
                    cloneMethod.Parameters.Add(new ParameterDefinition(_delegate));
                    cloneMethod.Body.InitLocals = true;
                    var tempVariable = new VariableDefinition(type);
                    cloneMethod.Body.Variables.Add(tempVariable);
                    var cInstr = cloneMethod.Body.Instructions;

                    // Create the copy using the copy constructor and put it in a local
                    cInstr.Add(Instruction.Create(OpCodes.Ldarg_0));
                    cInstr.Add(Instruction.Create(OpCodes.Newobj, copyConstructor));
                    cInstr.Add(Instruction.Create(OpCodes.Stloc, tempVariable));

                    // Set the argumentField to the lambda argument
                    if (lambdaExpression.Parameter != null)
                    {
                        cInstr.Add(Instruction.Create(OpCodes.Ldloc, tempVariable));
                        cInstr.Add(Instruction.Create(OpCodes.Ldarg_1));
                        cInstr.Add(Instruction.Create(OpCodes.Stfld, argumentField));
                    }

                    // Create a tuple containing the lambda return value nodes and return it
                    cInstr.Add(Instruction.Create(OpCodes.Ldloc, tempVariable));
                    cInstr.Add(Instruction.Create(OpCodes.Ldftn, _nodeInfos[lambdaExpression.ReturnValue1].Method));
                    cInstr.Add(Instruction.Create(OpCodes.Newobj, _delegate_ctor));
                    cInstr.Add(Instruction.Create(OpCodes.Ldloc, tempVariable));
                    cInstr.Add(Instruction.Create(OpCodes.Ldftn, _nodeInfos[lambdaExpression.ReturnValue2].Method));
                    cInstr.Add(Instruction.Create(OpCodes.Newobj, _delegate_ctor));
                    cInstr.Add(Instruction.Create(OpCodes.Newobj, _delegateTuple_ctor));
                    cInstr.Add(Instruction.Create(OpCodes.Ret));

                    _nodeInfos[lambdaExpression].CloneMethod = cloneMethod;
                    type.Methods.Add(cloneMethod);

                    i++;
                }
            }

            // Populate _callInfos
            {
                var i = 0;
                foreach (var node in nodes.AllNodes.OfType<CallOutputNode>())
                {
                    CreateTypeForFunctionAndRecurse(node.Call.Function);
                    if (!_callInfos.TryGetValue(node.Call, out var inf))
                    {
                        inf = _callInfos[node.Call] = new CallInfo(i, _functionTypes[node.Call.Function].Type, type);
                        i++;
                    }
                    inf.CallOutputNodes.Add(node);
                }
            }

            // Populate _lambdaInvocationInfos
            {
                var i = 0;
                foreach (var node in nodes.AllNodes.OfType<LambdaInvocationOutputNode>())
                {
                    if (!_lambdaInvocationInfos.TryGetValue(node.Invocation, out var inf))
                    {
                        inf = _lambdaInvocationInfos[node.Invocation] = new LambdaInvocationInfo(i, _delegateTuple, type);
                        i++;
                    }

                    if (node.OutputPosition == 1)   // direction 1 = → = output 2
                        inf.ReturnValue2Node = node;
                    else
                        inf.ReturnValue1Node = node;
                }
            }
        }

        private IEnumerable<object> GenerateIL(Node node, Func<TypeReference, VariableDefinition> getTempVariable, int depth, bool skipDic = false)
        {
            return !skipDic && _nodeInfos.TryGetValue(node, out var info) && info.Method != null
                ? [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldftn, info.Method),
                    Instruction.Create(OpCodes.Newobj, _delegate_ctor),
                    depth
                ]
                : node switch
                {
                    CallOutputNode callOut => GenerateILForCallOutputNode(callOut, getTempVariable, depth),
                    LiteralNode lit => GenerateILForLiteralNode(lit),
                    StdInNode _ => [Instruction.Create(OpCodes.Call, GetStdinMethod())],
                    NandNode nand => GenerateILForNandNode(nand, getTempVariable, depth),
                    InputNode inp => GenerateILForInputNode(inp, depth),
                    LessThanNode lt => GenerateILForLessThanNode(lt, getTempVariable, depth),
                    ShiftLeftNode shl => GenerateILForShiftLeftNode(shl, getTempVariable, depth),
                    LambdaExpressionNode λexpr => GenerateILForLambdaExpressionNode(λexpr),
                    LambdaExpressionParameterNode λparam => GenerateILForLambdaExpressionParameterNode(λparam, depth),
                    LambdaInvocationOutputNode λout => GenerateILForLambdaInvocationOutputNode(λout, getTempVariable, depth),
                    _ => throw new InvalidOperationException("Node type not recognized.")
                };
        }

        private IEnumerable<object> GenerateILForLambdaInvocationOutputNode(LambdaInvocationOutputNode node, Func<TypeReference, VariableDefinition> getTempVariable, int depth)
        {
            var branchCount = _branchCount++;
            var inf = _lambdaInvocationInfos[node.Invocation];
            var tupleField = inf.GetTupleField();

            // If tupleField == null, this lambda invocation uses only one of the outputs, in which case we don’t need to store the tuple in a field.
            if (tupleField != null)
            {
                // If there is a tupleField, we need to re-use its value if it is not null.
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, tupleField);
                yield return Instruction.Create(OpCodes.Dup);
                yield return Tuple.Create(OpCodes.Brtrue, "TUPLETRUE" + branchCount);
                yield return Instruction.Create(OpCodes.Pop);
            }

            // Evaluate LambdaGetter and convert its result to int
            foreach (var instr in GenerateIL(node.Invocation.LambdaGetter, getTempVariable, depth))
                yield return instr;
            var tempVariable = getTempVariable(_int);
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_Explicit_toInt);
            yield return Instruction.Create(OpCodes.Stloc, tempVariable);

            if (tupleField != null)
            {
                // (for later stfld tupleField)
                yield return Instruction.Create(OpCodes.Ldarg_0);
            }

            // Get the delegate from the _lambdaList
            yield return Instruction.Create(OpCodes.Ldsfld, _lambdaList);
            yield return Instruction.Create(OpCodes.Ldloc, tempVariable);
            yield return Instruction.Create(OpCodes.Callvirt, _lambdaListGetItem);

            // Instantiate the lambda argument delegate that will be the argument to the delegate from the list
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldftn, _nodeInfos[node.Invocation.Argument].Method);
            yield return Instruction.Create(OpCodes.Newobj, _delegate_ctor);

            // Invoke the delegate to get the tuple
            yield return Instruction.Create(OpCodes.Callvirt, _closureDelegateInvoke);

            if (tupleField != null)
            {
                // Store the tuple returned by the delegate in the tupleField
                yield return Instruction.Create(OpCodes.Stfld, tupleField);
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, tupleField);

                yield return Tuple.Create(OpCodes.Br, "TUPLE" + branchCount);

                // If we branched here because the tupleField was non-null, it means this is the second (and therefore last) time we need it, so null it so that it can be garbage-collected.
                yield return "TUPLETRUE" + branchCount;
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldnull);
                yield return Instruction.Create(OpCodes.Stfld, tupleField);

                yield return "TUPLE" + branchCount;
            }

            // Tuple is now on the stack. Extract the relevant delegate from it.
            yield return Instruction.Create(OpCodes.Callvirt, node.OutputPosition == 1 ? _delegateTupleGetItem2 : _delegateTupleGetItem1);
            yield return depth;
        }

        private IEnumerable<object> GenerateILForLambdaExpressionParameterNode(LambdaExpressionParameterNode node, int depth)
        {
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldfld, _nodeInfos[node].ArgumentField);
            yield return depth;
        }

        private IEnumerable<object> GenerateILForLambdaExpressionNode(LambdaExpressionNode node)
        {
            // Get the count of items in the list (i.e. the new lambda closure ID)
            yield return Instruction.Create(OpCodes.Ldsfld, _lambdaList);
            yield return Instruction.Create(OpCodes.Callvirt, _lambdaListCount);

            // Create the closure and add it to the list
            yield return Instruction.Create(OpCodes.Ldsfld, _lambdaList);
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldftn, _nodeInfos[node].CloneMethod);
            yield return Instruction.Create(OpCodes.Newobj, _closureDelegate_ctor);
            yield return Instruction.Create(OpCodes.Callvirt, _lambdaListAdd);

            // The lambda closure ID is now on the stack; convert it to BigInteger
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_int);
        }

        private IEnumerable<object> GenerateILForShiftLeftNode(ShiftLeftNode node, Func<TypeReference, VariableDefinition> getTempVariable, int depth)
        {
            var branchCount = _branchCount++;
            foreach (var instr in GenerateIL(node.Left, getTempVariable, depth))
                yield return instr;
            foreach (var instr in GenerateIL(node.Right, getTempVariable, depth + 1))
                yield return instr;

            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_Explicit_toInt);
            yield return Instruction.Create(OpCodes.Dup);
            yield return Instruction.Create(OpCodes.Ldc_I4_0);
            yield return Tuple.Create(OpCodes.Blt, "NEGATIVE" + branchCount);
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_LeftShift);
            yield return Tuple.Create(OpCodes.Br, "DONE" + branchCount);
            yield return "NEGATIVE" + branchCount;
            yield return Instruction.Create(OpCodes.Neg);
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_RightShift);
            yield return "DONE" + branchCount;
        }

        private IEnumerable<object> GenerateILForLessThanNode(LessThanNode node, Func<TypeReference, VariableDefinition> getTempVariable, int depth)
        {
            var branchCount = _branchCount++;
            foreach (var instr in GenerateIL(node.Left, getTempVariable, depth))
                yield return instr;
            foreach (var instr in GenerateIL(node.Right, getTempVariable, depth + 1))
                yield return instr;

            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_LessThan);
            yield return Tuple.Create(OpCodes.Brtrue, "MINUSONE" + branchCount);
            yield return Instruction.Create(OpCodes.Call, _bigInteger_get_Zero);
            yield return Tuple.Create(OpCodes.Br, "DONE" + branchCount);
            yield return "MINUSONE" + branchCount;
            yield return Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne);
            yield return "DONE" + branchCount;
        }

        private IEnumerable<object> GenerateILForInputNode(InputNode node, int depth) =>
        [
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, _inputFields[node]),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Stfld, _inputFields[node]),
            depth
        ];

        private IEnumerable<object> GenerateILForNandNode(NandNode node, Func<TypeReference, VariableDefinition> getTempVariable, int depth)
        {
            var branchCount = _branchCount++;

            // Optimize NAND with a literal 0
            if (node.Left is LiteralNode literal && literal.Result == 0)
            {
                yield return Instruction.Create(OpCodes.Call, _bigInteger_get_MinusOne);
                yield break;
            }

            // Evaluate left operand
            foreach (var instr in GenerateIL(node.Left, getTempVariable, depth))
                yield return instr;

            // Optimize bitwise-not
            if (node.Left == node.Right)
            {
                yield return Instruction.Create(OpCodes.Call, _bigInteger_op_OnesComplement);
                yield break;
            }

            // Short-circuit semantics
            yield return Instruction.Create(OpCodes.Dup);
            yield return Instruction.Create(OpCodes.Call, _bigInteger_get_Zero);
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_Equality);
            yield return Tuple.Create(OpCodes.Brtrue, "DONE" + branchCount);

            // Evaluate right operand
            foreach (var instr in GenerateIL(node.Right, getTempVariable, depth + 1))
                yield return instr;
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_BitwiseAnd);
            yield return "DONE" + branchCount;
            yield return Instruction.Create(OpCodes.Call, _bigInteger_op_OnesComplement);
        }

        private IEnumerable<object> GenerateILForLiteralNode(LiteralNode node)
        {
            if (node.Result <= int.MaxValue && node.Result >= int.MinValue)
            {
                yield return Instruction.Create(OpCodes.Ldc_I4, (int) node.Result);
                yield return Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_int);
            }
            else if (node.Result <= long.MaxValue && node.Result >= long.MinValue)
            {
                yield return Instruction.Create(OpCodes.Ldc_I8, (long) node.Result);
                yield return Instruction.Create(OpCodes.Call, _bigInteger_op_Implicit_long);
            }
            else
            {
                yield return Instruction.Create(OpCodes.Ldstr, node.Result.ToString());
                yield return Instruction.Create(OpCodes.Call, _bigInteger_Parse);
            }
        }

        private IEnumerable<object> GenerateILForCallOutputNode(CallOutputNode node, Func<TypeDefinition, VariableDefinition> getTempVariable, int depth)
        {
            var branchCount = _branchCount++;
            var inf = _callInfos[node.Call];

            var constructorInstr = new List<object>();
            foreach (var input in node.Call.Function.FindNodes().AllNodes.OfType<InputNode>().OrderBy(inp => inp.InputPosition))
            {
                constructorInstr.Add(Instruction.Create(OpCodes.Ldarg_0));
                constructorInstr.Add(Instruction.Create(OpCodes.Ldftn, _nodeInfos[node.Call.Inputs[input.InputPosition]].Method));
                constructorInstr.Add(Instruction.Create(OpCodes.Newobj, _delegate_ctor));
            }
            constructorInstr.Add(Instruction.Create(OpCodes.Newobj, _functionTypes[node.Call.Function].Constructor));

            if (inf.CallOutputNodes.Count == 1)
            {
                // The function has only one output: we don’t need to store the function instance in a field
                foreach (var instr in constructorInstr)
                    yield return instr;
            }
            else
            {
                // The function has multiple outputs: we have to place the function instance in a field in order to ensure that it is instantiated only once
                var fields = inf.GetFields();
                var tempVariable = getTempVariable(_functionTypes[node.Call.Function].Type);
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, fields[node.OutputPosition]);
                yield return Instruction.Create(OpCodes.Dup);
                yield return Tuple.Create(OpCodes.Brtrue, "TRUE" + branchCount);
                yield return Instruction.Create(OpCodes.Pop);
                foreach (var instr in constructorInstr)
                    yield return instr;
                yield return Instruction.Create(OpCodes.Dup);
                yield return Instruction.Create(OpCodes.Stloc, tempVariable);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == null || i == node.OutputPosition)
                        continue;
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldloc, tempVariable);
                    yield return Instruction.Create(OpCodes.Stfld, fields[i]);
                }
                yield return "TRUE" + branchCount;

                // Set the relevant field to null after evaluation so that the function instance can be garbage-collected (otherwise the entire execution tree would stay in memory)
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldnull);
                yield return Instruction.Create(OpCodes.Stfld, fields[node.OutputPosition]);
            }
            yield return Instruction.Create(OpCodes.Ldftn, _nodeInfos[node.Call.Function.OutputNodes[node.OutputPosition]].Method);
            yield return Instruction.Create(OpCodes.Newobj, _delegate_ctor);
            yield return depth;
        }

        private sealed class FunctionTypeInfo
        {
            public TypeDefinition Type;
            public MethodDefinition Constructor;
            public MethodDefinition CopyConstructor;
        }

        private sealed class NodeInfo
        {
            public MethodDefinition Method;

            public MethodDefinition CloneMethod;    // for lambda expression nodes only
            public FieldDefinition ArgumentField;      // for lambda expression parameter nodes only

            private readonly Dictionary<TypeReference, VariableDefinition> _temporaryLocals = [];

            public VariableDefinition CreateTemporaryLocal(TypeReference type)
            {
                if (_temporaryLocals.TryGetValue(type, out var tempVariable))
                    return tempVariable;

                tempVariable = new VariableDefinition(type);
                Method.Body.Variables.Add(tempVariable);
                _temporaryLocals[type] = tempVariable;
                return tempVariable;
            }
        }

        private sealed class CallInfo(int id, TypeReference functionType, TypeDefinition addFieldsToType)
        {
            public List<CallOutputNode> CallOutputNodes = [];
            private FieldDefinition[] _getFieldsCache;
            public FieldDefinition[] GetFields()
            {
                if (_getFieldsCache == null)
                {
                    _getFieldsCache = new FieldDefinition[4];
                    foreach (var outputNode in CallOutputNodes)
                    {
                        var field = new FieldDefinition(id.ToString() + "↑→↓←"[outputNode.OutputPosition], FieldAttributes.Private, functionType);
                        addFieldsToType.Fields.Add(field);
                        _getFieldsCache[outputNode.OutputPosition] = field;
                    }
                }
                return _getFieldsCache;
            }
        }

        private sealed class LambdaInvocationInfo(int id, TypeReference tupleType, TypeDefinition addTupleFieldToType)
        {
            public LambdaInvocationOutputNode ReturnValue1Node;
            public LambdaInvocationOutputNode ReturnValue2Node;
            private FieldDefinition _getTupleFieldCache;
            public FieldDefinition GetTupleField()
            {
                if (ReturnValue1Node == null || ReturnValue2Node == null)
                    return null;

                if (_getTupleFieldCache == null)
                {
                    _getTupleFieldCache = new FieldDefinition(id.ToString() + "∬", FieldAttributes.Private, tupleType);
                    addTupleFieldToType.Fields.Add(_getTupleFieldCache);
                }
                return _getTupleFieldCache;
            }
        }
    }
}
