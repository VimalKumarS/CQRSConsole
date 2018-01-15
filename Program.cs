using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class PersonStorage
    {
        Dictionary<int, Person> people;
    }
    public class Person
    {
        public int UniqueID;
        private int Age;
        EventBroker broker;
        public Person(EventBroker broker)
        {
            this.broker = broker;
            broker.Commands += Broker_Commands;
            broker.Queries += Broker_Queries;
        }

        private void Broker_Queries(object sender, Query e)
        {
            var ac = e as AgeQuery;
            if(ac!=null && ac.target == this)
            {
                ac.Result = Age;
            }
        }

        private void Broker_Commands(object sender, Command e)
        {
            //throw new NotImplementedException();
            var cac = e as ChangeAgeCommand;
            if(cac != null && cac.Target == this)
            {
               if(cac.Register) broker.AllEvents.Add(new AgeChangeEvent(this, Age, cac.Age));
                Age = cac.Age;
            }
        }

        public bool CanVote => Age >= 16; 
    }
     
    public class EventBroker
    {
        public IList<Event> AllEvents = new List<Event>();
        public event EventHandler<Command> Commands;
        public event EventHandler<Query> Queries;

        public void Command(Command c)
        {
            Commands?.Invoke(this, c);
        }

        public T Query<T>(Query q)
        {
            Queries?.Invoke(this, q);
            return (T)q.Result;
        }

        public void UndoList()
        {
            var e = AllEvents.LastOrDefault();
            var ac = e as AgeChangeEvent;
            if(ac != null)
            {
                Command(new ChangeAgeCommand(ac.Target, ac.Oldvalue) {Register=false  });
                AllEvents.Remove(e);
            }
        }
    }


    public class Event
    {

    }

    public class Command :EventArgs
    {
        public bool Register = true;
    }

    class AgeChangeEvent : Event
    {
        public Person Target;
        public int Oldvalue, Newvalue;
        public AgeChangeEvent(Person target,int oldvalue, int newvalue)
        {
            Target = target;
            Oldvalue = oldvalue;
            Newvalue = newvalue;
        }

        public override string ToString()
        {
            return $"Age change from {Oldvalue} to {Newvalue}";
        }
    }

    public class Query {
        public object Result;
    }

    public class AgeQuery :Query {
        public Person target;
    }

    class ChangeAgeCommand : Command
    {
        public Person Target;
        public int Age;

        public ChangeAgeCommand(Person target, int age)
        {
            Target = target;
            Age = age;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var eb = new EventBroker(); ;
            var p = new Person(eb);
            eb.Command(new ChangeAgeCommand(p, 10));
            int age = eb.Query<int>(new AgeQuery { target = p });
            Console.ReadKey();
        }
    }
}
