using System;
using System.Collections.Generic;

namespace Entitites
{
    [Serializable]
    public class User : IEqualityComparer<User>, IEquatable<User>
    {
        private string firstname;
        private string lastname;
        public int Id { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string FirstName
        {
            get
            {
                return firstname;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                firstname = value;
            }
        }

        public string LastName
        {
            get
            {
                return lastname;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                lastname = value;
            }
        }
        public List<Visa> VisaRecords = new List<Visa>();

        public User(string firstName, string lastName, DateTime dateOfBirth)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
        }

        public User(string firstName, string lastName, DateTime dateOfBirth, int id) : this(firstName, lastName, dateOfBirth)
        {
            Id = id;
        }

        public bool Equals(User x, User y)
        {
            return x.FirstName == y.FirstName && x.LastName == y.LastName && x.DateOfBirth.Equals(y.DateOfBirth);
        }

        public int GetHashCode(User obj)
        {
            return ToString().GetHashCode();
        }

        public bool Equals(User other)
        {
            return Equals(this, other);
        }

        public override string ToString()
        {
            return $"Id = {Id}, FirstName = {FirstName}, LastName = {LastName}, DateOfBirth = {DateOfBirth}";
        }
    }
}
