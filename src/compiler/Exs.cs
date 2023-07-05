using System.Diagnostics.CodeAnalysis;

namespace flashsolve.compiler; 

public class UnrecognizedAstNode : Exception {
    public UnrecognizedAstNode() { }
    public UnrecognizedAstNode(string msg) : base(msg) { }
    public UnrecognizedAstNode(string msg, Exception inner) : base(msg, inner) { }

    [DoesNotReturn]
    public static Object Throw(Object o) {
        var t = o.GetType();
        throw new UnrecognizedAstNode($"Ast Node of type ${t.Name} is unrecognized, no code to handle it.");
    }
}

public class UnrecognizedAstPropertyValue : Exception {
    public UnrecognizedAstPropertyValue() { }
    public UnrecognizedAstPropertyValue(string msg) : base(msg) { }
    public UnrecognizedAstPropertyValue(string msg, Exception inner) : base(msg, inner) { }

    [DoesNotReturn]
    public static Object Throw(Object o) {
        var t = o.GetType();
        throw new UnrecognizedAstPropertyValue($"Ast Node of type ${t.Name} can't have the value ${o}, it's an unrecognized value and there is no code to handle it.");
    }
}

public class UnsupportedOperation : Exception {
    public UnsupportedOperation() { }
    public UnsupportedOperation(string msg) : base(msg) { }
    public UnsupportedOperation(string msg, Exception inner) : base(msg, inner) { }
}

public class TypeMismatch : Exception {
    public TypeMismatch() { }
    public TypeMismatch(string msg) : base(msg) { }
    public TypeMismatch(string msg, Exception inner) : base(msg, inner) { }
}

public class UnrecognizedNumberFormat : Exception {
    public UnrecognizedNumberFormat() { }
    public UnrecognizedNumberFormat(string msg) : base(msg) { }
    public UnrecognizedNumberFormat(string msg, Exception inner) : base(msg, inner) { }
}

public class NoSuchChildClass : Exception {
    public NoSuchChildClass() { }
    public NoSuchChildClass(string msg) : base(msg) { }
    public NoSuchChildClass(string msg, Exception inner) : base(msg, inner) { }
}