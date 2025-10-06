using Foundation.Business.DomainNotitications;
using Foundation.Business.Model;
using Transactions.Business.Events;

namespace Transactions.Business.Model
{
    public class Transaction : Notifiable, IEntity
    {
        public static readonly string VALUE_MUST_HAVE_GRATHER_THAN_0 = "Value must have grather than 0";

        private Guid _id;
        public Guid Id => _id;

        private string _description;
        public string Description => _description;

        private Double _value;
        public double Value => _value;

        private TransactionType _type;
        public TransactionType Type => _type;

        private DateTime _date;
        public DateTime Date => _date;               

        public TransactionCreatedEvent Create(string  description, double value, TransactionType type)
        {
            _id = Guid.NewGuid();
            _description = description;
            _value = value;
            _type = type;
            _date = DateTime.Now;

            return new TransactionCreatedEvent(_id, _description, _value, _date, _type);            
        }

        public override void Validate()
        {
            ValueMustHaveGratherThan0Spec();
        }

        private void ValueMustHaveGratherThan0Spec()
        {
            if(_value <= 0)
            {
                AddNotification(VALUE_MUST_HAVE_GRATHER_THAN_0);
            }
        }    
    }
}