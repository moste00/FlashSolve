using System.Collections;
using Antlr4.Runtime;

namespace flashsolve.util; 

public static class StrUtils {
    //low level class, shouldn't be constructed directly, makes a lot of assumptions about the caller
    public class SubString {
        private string _original;
        private uint _start;
        //INT32.MINValue means till the end of the string
        private int _end;
        //doesn't perform bounds checking
        //holds a reference to the string it's looking onto, so beware of unexpected GC effects
        public SubString(string originalStr,uint start, int end) {
            _original = originalStr;
            _start = start;
            _end = end;
        }
        
        public abstract class ConversionToBoolArrFailure: Exception {
            public ConversionToBoolArrFailure() { }
            public ConversionToBoolArrFailure(string msg) : base(msg) { }
            public ConversionToBoolArrFailure(string msg, Exception inner) : base(msg, inner) { }
            public class NotEnoughBits : ConversionToBoolArrFailure {
                public NotEnoughBits() { }
                public NotEnoughBits(string msg) : base(msg) { }
                public NotEnoughBits(string msg, Exception inner) : base(msg, inner) { }
            }

            public class UnrecognizedCharacter : ConversionToBoolArrFailure {
                public UnrecognizedCharacter() { }
                public UnrecognizedCharacter(string msg) : base(msg) { }
                public UnrecognizedCharacter(string msg, Exception inner) : base(msg, inner) { }
            }
        }
        public bool[] HexToBoolArr(uint arraysz) {
            int strlen = _original.Length;
            
            bool[] result = new bool[arraysz];
            if (arraysz < strlen * 4) {
                throw new ConversionToBoolArrFailure.NotEnoughBits(
                    $"A hex number of length {strlen} can't fit in less than {4 * strlen} bits. (given size was {arraysz})");
            }
            int endIndex = EndIndex(_end, strlen);
            int resultIndex = endIndex * 4 - 1;
            
            
            for (uint i = _start; i <= endIndex; i++) {
                char currDigit = _original[(int)i];
                switch (currDigit) {
                    //0000
                    case '0':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //0001
                    case '1':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true ;
                        break;
                    //0010
                    case '2':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //0011
                    case '3':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    //0100
                    case '4':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //0101
                    case '5':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true;
                        break;
                    //0110
                    case '6':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //0111
                    case '7':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    //1000
                    case '8':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //1001
                    case '9':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true;
                        break;
                    //1010
                    case 'a':
                    case 'A':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //1011
                    case 'b':
                    case 'B':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    //1100
                    case 'c':
                    case 'C':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //1101
                    case 'd':
                    case 'D': 
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true;
                        break;
                    //1110
                    case 'e':
                    case 'E': 
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //1111
                    case 'f':
                    case 'F': 
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    default:
                        throw new ConversionToBoolArrFailure.UnrecognizedCharacter(
                            $"Character {_original[(int)i]} is not a valid hex digit.");
                }

                resultIndex -= 4;
            }

            return result;
        }

        public bool[] OctToBoolArr(uint arraysz) {
            int strlen = _original.Length;
            
            bool[] result = new bool[arraysz];
            if (arraysz < strlen * 4) {
                throw new ConversionToBoolArrFailure.NotEnoughBits(
                    $"A hex number of length {strlen} can't fit in less than {4 * strlen} bits. (given size was {arraysz})");
            }
            int endIndex = EndIndex(_end, strlen);
            int resultIndex = endIndex * 4 - 1;
            
            
            for (uint i = _start; i <= endIndex; i++) {
                char currDigit = _original[(int)i];
                switch (currDigit) {
                    //000
                    case '0':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        break;
                    //001
                    case '1':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true ;
                        break;
                    //010
                    case '2':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        break;
                    //011
                    case '3':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        break;
                    //100
                    case '4':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        break;
                    //101
                    case '5':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        break;
                    //110
                    case '6':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        break;
                    //111
                    case '7':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        break;
                    default:
                        throw new ConversionToBoolArrFailure.UnrecognizedCharacter(
                            $"Character {_original[(int)i]} is not a valid hex digit.");
                }

                resultIndex -= 3;
            }

            return result;
        }

        public bool[] BinToBoolArr(uint arraysz) {
            int strlen = _original.Length;
            
            bool[] result = new bool[arraysz];
            if (arraysz < strlen * 4) {
                throw new ConversionToBoolArrFailure.NotEnoughBits(
                    $"A hex number of length {strlen} can't fit in less than {4 * strlen} bits. (given size was {arraysz})");
            }
            int endIndex = EndIndex(_end, strlen);
            int resultIndex = endIndex * 4 - 1;
            
            
            for (uint i = _start; i <= endIndex; i++) {
                char currDigit = _original[(int)i];
                switch (currDigit) {
                    case '0':
                        result[resultIndex] = false;
                        break;
                    case '1':
                        result[resultIndex] = true;
                        break;
                    default:
                        throw new ConversionToBoolArrFailure.UnrecognizedCharacter(
                            $"Character {_original[(int)i]} is not a valid hex digit.");
                }
                resultIndex--;
            }

            return result;
        }
        
        public static int EndIndex(int end, int strlen) {
            if (end == Int32.MinValue) {
                return strlen - 1;
            }
            if (end < 0) {
                return strlen + end;
            }
            return end;
        }

        public ReversedSubString Reversed => new(_original, _start, _end);
    }

    public static class SubStringBuilders {
        //the type state pattern
        public class OriginalStrBState {
            private string _original;

            public OriginalStrBState(string s) {
                _original = s;
            }

            public OriginalStrStartBState From(uint start) {
                return new OriginalStrStartBState(_original, start);
            }

            public OriginalStrStartBState FromStart => From(0);
        }

        public class OriginalStrStartBState {
            private string _original;
            private uint _start;

            public OriginalStrStartBState(string s,uint start) {
                _original = s;
                _start = start;
            }

            public SubString To(int end) => new(_original, _start, end);
            public SubString ToEnd => To(Int32.MinValue);
        }
    }

    public class ReversedSubString : IEnumerable<char> {
        private string _original;
        private uint _start;
        //INT32.MINValue means till the end of the string
        private int _end;
        //doesn't perform bounds checking
        //holds a reference to the string it's looking onto, so beware of unexpected GC effects
        public ReversedSubString(string originalStr,uint start, int end) {
            _original = originalStr;
            _start = start;
            _end = end;
        }
        
        public bool[] HexToBoolArr(uint arraysz) {
            int strlen = _original.Length;
            
            bool[] result = new bool[arraysz];
            if (arraysz < strlen * 4) {
                throw new SubString.ConversionToBoolArrFailure.NotEnoughBits(
                    $"A hex number of length {strlen} can't fit in less than {4 * strlen} bits. (given size was {arraysz})");
            }
            int endIndex = SubString.EndIndex(_end, strlen);
            int resultIndex = endIndex * 4 - 1;
            
            
            for (int i = endIndex; i >= _start; i--) {
                char currDigit = _original[(int)i];
                switch (currDigit) {
                    //0000
                    case '0':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //0001
                    case '1':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true ;
                        break;
                    //0010
                    case '2':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //0011
                    case '3':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    //0100
                    case '4':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //0101
                    case '5':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true;
                        break;
                    //0110
                    case '6':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //0111
                    case '7':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    //1000
                    case '8':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //1001
                    case '9':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true;
                        break;
                    //1010
                    case 'a':
                    case 'A':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //1011
                    case 'b':
                    case 'B':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    //1100
                    case 'c':
                    case 'C':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = false;
                        break;
                    //1101
                    case 'd':
                    case 'D': 
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        result[resultIndex - 3] = true;
                        break;
                    //1110
                    case 'e':
                    case 'E': 
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = false;
                        break;
                    //1111
                    case 'f':
                    case 'F': 
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        result[resultIndex - 3] = true;
                        break;
                    default:
                        throw new SubString.ConversionToBoolArrFailure.UnrecognizedCharacter(
                            $"Character {_original[(int)i]} is not a valid hex digit.");
                }

                resultIndex -= 4;
            }

            return result;
        }

        public bool[] OctToBoolArr(uint arraysz) {
            int strlen = _original.Length;
            
            bool[] result = new bool[arraysz];
            if (arraysz < strlen * 4) {
                throw new SubString.ConversionToBoolArrFailure.NotEnoughBits(
                    $"A hex number of length {strlen} can't fit in less than {4 * strlen} bits. (given size was {arraysz})");
            }
            int endIndex = SubString.EndIndex(_end, strlen);
            int resultIndex = endIndex * 4 - 1;
            
            
            for (int i = endIndex; i >= _start; i--) {
                char currDigit = _original[(int)i];
                switch (currDigit) {
                    //000
                    case '0':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        break;
                    //001
                    case '1':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true ;
                        break;
                    //010
                    case '2':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        break;
                    //011
                    case '3':
                        result[resultIndex    ] = false;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        break;
                    //100
                    case '4':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = false;
                        break;
                    //101
                    case '5':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = false;
                        result[resultIndex - 2] = true;
                        break;
                    //110
                    case '6':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = false;
                        break;
                    //111
                    case '7':
                        result[resultIndex    ] = true;
                        result[resultIndex - 1] = true;
                        result[resultIndex - 2] = true;
                        break;
                    default:
                        throw new SubString.ConversionToBoolArrFailure.UnrecognizedCharacter(
                            $"Character {_original[(int)i]} is not a valid hex digit.");
                }

                resultIndex -= 3;
            }

            return result;
        }

        public bool[] BinToBoolArr(uint arraysz) {
            int strlen = _original.Length;
            
            bool[] result = new bool[arraysz];
            if (arraysz < strlen * 4) {
                throw new SubString.ConversionToBoolArrFailure.NotEnoughBits(
                    $"A hex number of length {strlen} can't fit in less than {4 * strlen} bits. (given size was {arraysz})");
            }
            int endIndex = SubString.EndIndex(_end, strlen);
            int resultIndex = endIndex * 4 - 1;
            
            
            for (int i = endIndex; i >= _start; i--) {
                char currDigit = _original[(int)i];
                switch (currDigit) {
                    case '0':
                        result[resultIndex] = false;
                        break;
                    case '1':
                        result[resultIndex] = true;
                        break;
                    default:
                        throw new SubString.ConversionToBoolArrFailure.UnrecognizedCharacter(
                            $"Character {_original[(int)i]} is not a valid hex digit.");
                }
                resultIndex--;
            }

            return result;
        }
        
        public IEnumerator<char> GetEnumerator() {
            int endIndex = SubString.EndIndex(_end, _original.Length);
            for (int i = endIndex; i >= _start; i--) {
                yield return _original[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
    public static SubStringBuilders.OriginalStrBState Slice(this string s) {
        return new SubStringBuilders.OriginalStrBState(s);
    }
}