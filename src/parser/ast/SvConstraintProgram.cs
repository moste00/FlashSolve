/*
 * A "Constraint Program" is the name we give to a self-contained collection of SystemVerilog classes/interfaces containing constraints.
 * i.e. We are only interested in the subset of SystemVerilog that can specify constraints (classes and interfaces), and we are only interested in representing
 *      and transforming (and ultimately solving) those constraints, not any other aspect of the (huge) SV language.
 */
namespace FlashSolve.parser.ast;


public class SvConstraintProgram {
    private readonly List<SvClass> _classes ;

    public SvConstraintProgram() => _classes = new List<SvClass>();

    public SvClass this[int i] {
        get => _classes[i];
        set => _classes[i] = value;
    }

    public void AddClass(SvClass cls) => _classes.Add(cls);
}