# FlashSolve
FlashSolve is an open source Constrained Random Verification (CRV) tool. CRV is a testing methodology, used in both software and hardware, that aims to systematically generate test stimulus (test cases) to feed into the System Under Test (SUT). This is a much more robust and powerful approach to testing than manual testing, scenario-based testing, snapshot testing, or unit testing. By systematically defining the input space for the SUT using a mathematical constraint language, then using a constraint solver to exhaustively test the SUT using inputs from the input space, or to randomly sample inputs in case the input space is too large for exhaustive testing.

Our project aims at enhancing the solution generation process for SystemVerilog (SV) constraints. The motivation behind this project is to develop an open-source tool that can be utilized by a wide range of users. SystemVerilog, a hardware description language (HDL), contains a sub-language for constraint definition and random variable definition, and SV runtimes contains an embedded solver for this constraint language. Our project aims to be such a solver, in stand-alone form that can be embedded in any SV runtime or toolchain.

By combining the power of ANTLR, the .NET runtime, and the state-of-the-art Z3 SMT solver, Our project offers a versatile and user-friendly tool for generating random solutions to SV constraints. The open-source nature of this project ensures accessibility, fostering further advancements in the field of hardware description languages and constraint solving techniques.

Our project is architected as an additional layer on top of Z3, implementing various innovative techniques for random sampling of SMT constraints. These techniques include a naive approach, Max-SMT, Universal Hashing, and a hybrid approach that combines these techniques. By incorporating these enhancements, we are able to sample random solutions by guiding the Z3 solver to random subsets of the input space (i.e. in a black-box fashion, with no modification of the solver itself), thereby significantly improving the solution generation process.

### 🔗 Dependencies
#### - .Net
install .Net 7.0.7 (sdk-7.0.304) from Microsoft official website:    
```
https://dotnet.microsoft.com/en-us/download/dotnet/7.0
```
or click [here](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
#### - ANTLR
Download the jar file for ANTLR from [here](https://github.com/antlr/website-antlr4/blob/gh-pages/download/antlr-4.11.1-complete.jar) or [here](https://drive.google.com/drive/folders/1-vsthF-27fyy6HhUfgucQaZPaHfZiG_O?usp=sharing)

### 💿 How to run
#### There are 4 modes to run:
#### 1. parse: to parse SV file
write in the cmd the following
```
dotnet run parse file_path
```
example:
``` 
dotnet run parse "Tests/ifelse1.txt"
```
**Note**: you can feed the program in sv extension also
#### 2. compile: to generate the Z3 problem (parse then compile)
write in the cmd the following
```
dotnet run compile file_path
```
example:
``` 
dotnet run compile "Tests/ifelse1.txt"
```
**Note**: you can feed the program in sv extension also

#### 3. sample: to run the sampling algorithms you choose on one file (parse then compile then sample)
write in the cmd the following
```
dotnet run sample number_of_sols file_path
```
example:
``` 
dotnet run sample 50 "Tests/ifelse1.txt"
```
**Note**: you can feed the program in sv extension also
#### 4. test: sample on different test cases
**Note:** Put test files in **Tests** folder
write in the cmd the following
```
dotnet run test
```
**Note:** in the third and fourth case you will have the results and benchmark in **out** directory
**Note:** the benchmark file only has the time of sampling, to calculate the spread you have to run another script

#### Calculate the Spread
- run python script (spread_from_results.py) 
```
python script_file_path outfile_path
```