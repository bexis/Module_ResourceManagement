using BExIS.Rbm.Entities.Users;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.Users
{
    public class PersonManager :IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public PersonManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.IndividualPersonRepo = _guow.GetReadOnlyRepository<IndividualPerson>();
            this.PersonGroupRepo = _guow.GetReadOnlyRepository<PersonGroup>();
            this.PersonRepo = _guow.GetReadOnlyRepository<Person>();

        }

        ~PersonManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        #region Data Readers

        public IReadOnlyRepository<IndividualPerson> IndividualPersonRepo { get; private set; }
        public IReadOnlyRepository<PersonGroup> PersonGroupRepo { get; private set; }
        public IReadOnlyRepository<Person> PersonRepo { get; private set; }



        #endregion

        public bool DeletePerson(Person person)
        {
            Contract.Requires(person != null);
            Contract.Requires(person.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Person> repo = uow.GetRepository<Person>();
                person = repo.Reload(person.Self);
                repo.Delete(person);
                uow.Commit();
            }

            return true;
        }

        #region IndividualPerson

        /// <summary>
        /// Creates an IndividualPerson <seealso cref="IndividualPerson"/> and persists the entity in the database.
        /// </summary>
        public IndividualPerson CreateIndividualPerson(User user)
        {
            IndividualPerson individualPerson = new IndividualPerson(user);
            individualPerson.Contact = user;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<IndividualPerson> repo = uow.GetRepository<IndividualPerson>();
                repo.Put(individualPerson);
                uow.Commit();
            }

            return individualPerson;
        }

        public IndividualPerson UpdateIndividualPerson(IndividualPerson individualPerson)
        {
            Contract.Requires(individualPerson != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<IndividualPerson> repo = uow.GetRepository<IndividualPerson>();
                repo.Merge(individualPerson);
                var merged = repo.Get(individualPerson.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return individualPerson;
        }


        public bool DeleteIndividualPerson(IndividualPerson individualPerson)
        {
            Contract.Requires(individualPerson != null);
            Contract.Requires(individualPerson.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<IndividualPerson> repo = uow.GetRepository<IndividualPerson>();
                individualPerson = repo.Reload(individualPerson);
                repo.Delete(individualPerson);
                uow.Commit();
            }

            return true;
        }

        public IndividualPerson GetIndividualPersonByUserId(long id)
        {
            return IndividualPersonRepo.Query(u => u.Person.Id == id).FirstOrDefault();
        }

        public IndividualPerson GetIndividualPersonById(long id)
        {
            return IndividualPersonRepo.Query(u => u.Person.Id == id).FirstOrDefault();
        }


        #endregion

        #region PersonGroup

        /// <summary>
        /// Creates an PersonGroup <seealso cref="PersonGroup"/> and persists the entity in the database.
        /// </summary>
        public PersonGroup CreatePersonGroup(List<User> users, User contact)
        {
            PersonGroup personGroup = new PersonGroup();
            personGroup.Users = users;
            personGroup.Contact = contact;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PersonGroup> repo = uow.GetRepository<PersonGroup>();
                repo.Put(personGroup);
                uow.Commit();
            }

            return personGroup;
        }

        /// <summary>
        /// Creates an PersonGroup <seealso cref="PersonGroup"/> without contact information and persists the entity in the database.
        /// </summary>
        public PersonGroup CreatePersonGroup(List<User> users)
        {
            PersonGroup personGroup = new PersonGroup();
            personGroup.Users = users;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PersonGroup> repo = uow.GetRepository<PersonGroup>();
                repo.Put(personGroup);
                uow.Commit();
            }

            return personGroup;
        }

        public PersonGroup UpdatePersonGroup(PersonGroup personGroup)
        {
            Contract.Requires(personGroup != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PersonGroup> repo = uow.GetRepository<PersonGroup>();
                repo.Merge(personGroup);
                var merged = repo.Get(personGroup.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return personGroup;
        }

        public bool DeletePersonGroup(PersonGroup personGroup)
        {
            Contract.Requires(personGroup != null);
            Contract.Requires(personGroup.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PersonGroup> repo = uow.GetRepository<PersonGroup>();
                personGroup = repo.Reload(personGroup);
                repo.Delete(personGroup);
                uow.Commit();
            }

            return true;
        }

        public PersonGroup GetPersonGroupById(long id)
        {
            return PersonGroupRepo.Query(u => u.Id == id).FirstOrDefault();
        }

        #endregion

        #region Person

        public Person UpdatePerson(Person person)
        {
            Contract.Requires(person != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Person> repo = uow.GetRepository<Person>();
                repo.Merge(person);
                var merged = repo.Get(person.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return person;
        }

        public Person GetPersonById(long id)
        {
            return PersonRepo.Query(u => u.Id == id).FirstOrDefault();
        }

        #endregion
    }
}
