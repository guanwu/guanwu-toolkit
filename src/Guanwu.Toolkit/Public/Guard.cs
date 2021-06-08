using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Guanwu.Toolkit
{
    /// <summary>
    /// A general inspection guard.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Asserts that a type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argumentName">The name of the type.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="OverflowException" />
        public static void TypeHasDefaultConstructor(Type type, string argumentName)
        {
            if (type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .All(ctor => ctor.GetParameters().Length != 0)) {
                throw new ArgumentException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        /// <param name="argumentName">The name of the value.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException" />
        public static void AgainstNull(string argumentName, object value)
        {
            if (value == null) {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a value is not null or empty.
        /// </summary>
        /// <param name="argumentName">The name of the value.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException" />
        public static void AgainstNullAndEmpty(string argumentName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a collection is not null or blank.
        /// </summary>
        /// <param name="argumentName">The name of the collection.</param>
        /// <param name="value">The collection.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public static void AgainstNullAndEmpty(string argumentName, ICollection value)
        {
            if (value == null) {
                throw new ArgumentNullException(argumentName);
            }
            if (value.Count == 0) {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a integer is greater than zero.
        /// </summary>
        /// <param name="argumentName">The name of the integer.</param>
        /// <param name="value">The integer.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static void AgainstNegativeAndZero(string argumentName, int value)
        {
            if (value <= 0) {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a integer is not negative.
        /// </summary>
        /// <param name="argumentName">The name of the integer.</param>
        /// <param name="value">The integer.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static void AgainstNegative(string argumentName, int value)
        {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a time interval is greater than zero.
        /// </summary>
        /// <param name="argumentName">The name of the time interval.</param>
        /// <param name="value">The time interval.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static void AgainstNegativeAndZero(string argumentName, TimeSpan value)
        {
            if (value <= TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Asserts that a time interval is not negative.
        /// </summary>
        /// <param name="argumentName">The name of the time interval.</param>
        /// <param name="value">The time interval.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static void AgainstNegative(string argumentName, TimeSpan value)
        {
            if (value < TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
    }
}