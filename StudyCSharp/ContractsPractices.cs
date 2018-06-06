#define CONTRACTS_FULL

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace StudyCSharp
{
    public class ContractsPractices
    {
        /// <summary>
        /// Debugging entrance.
        /// </summary>
        public enum CookPasta { BoilWater, AddPasta, AddSalt, Stir, Strain, Done }

        /// <summary>
        /// Example of CodeContractsHelpers.ThrowIfReached.
        /// the following code was copied from:
        /// https://blogs.msdn.microsoft.com/francesco/2014/09/12/how-to-use-cccheck-to-prove-no-case-is-forgotten/
        /// </summary>
        public static CookPasta Next(CookPasta state)
        {
            switch (state)
            {
                case CookPasta.BoilWater:
                    return CookPasta.AddPasta;

                case CookPasta.AddPasta:
                    return CookPasta.AddSalt;

                case CookPasta.AddSalt:
                    return CookPasta.Stir;

                case CookPasta.Stir:
                    return CookPasta.Strain;

                case CookPasta.Strain:
                    return CookPasta.Done;

                case CookPasta.Done:
                    return CookPasta.Done;

                default:
                    throw CodeContractsHelpers.ThrowIfReached("Unknown pasta state!");
            }
        }

        public static class CodeContractsHelpers
        {
            [ContractVerification(false)]
            public static Exception ThrowIfReached(string s)
            {
                Contract.Requires(false);

                return new Exception(s);
            }
        }

        /// <summary>
        /// the DiagnosticPractices class.
        /// the following code was copied from:
        /// https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/code-contracts
        /// </summary>
        // An IArray is an ordered collection of objects.    
        [ContractClass(typeof(IArrayContract))]
        public interface IArray
        {
            // The Item property provides methods to read and edit entries in the array.
            Object this[int index]
            {
                get;
                set;
            }

            int Count
            {
                get;
            }

            // Adds an item to the list.  
            // The return value is the position the new element was inserted in.
            int Add(Object value);

            // Removes all items from the list.
            void Clear();

            // Inserts value into the array at position index.
            // index must be non-negative and less than or equal to the 
            // number of elements in the array.  If index equals the number
            // of items in the array, then value is appended to the end.
            void Insert(int index, Object value);


            // Removes the item at position index.
            void RemoveAt(int index);
        }

        [ContractClassFor(typeof(IArray))]
        internal abstract class IArrayContract : IArray
        {
            int IArray.Add(Object value)
            {
                // Returns the index in which an item was inserted.
                Contract.Ensures(Contract.Result<int>() >= -1);
                Contract.Ensures(Contract.Result<int>() < ((IArray)this).Count);

                return default(int);
            }

            Object IArray.this[int index]
            {
                get
                {
                    Contract.Requires(index >= 0);
                    Contract.Requires(index < ((IArray)this).Count);
                    return default(int);
                }
                set
                {
                    Contract.Requires(index >= 0);
                    Contract.Requires(index < ((IArray)this).Count);
                }
            }

            public int Count
            {
                get
                {
                    Contract.Requires(Count >= 0);
                    Contract.Requires(Count <= ((IArray)this).Count);
                    return default(int);
                }
            }

            void IArray.Clear()
            {
                Contract.Ensures(((IArray)this).Count == 0);
            }

            void IArray.Insert(int index, Object value)
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index <= ((IArray)this).Count);  // For inserting immediately after the end.
                Contract.Ensures(((IArray)this).Count == Contract.OldValue(((IArray)this).Count) + 1);
            }

            void IArray.RemoveAt(int index)
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < ((IArray)this).Count);
                Contract.Ensures(((IArray)this).Count == Contract.OldValue(((IArray)this).Count) - 1);
            }
        }
    }
}
