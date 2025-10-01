namespace CashFlow.Entries.Domain.Entities
{
    public abstract class Entity
    {
        public List<string> Errors { get; private set; }

        public abstract void Validate();

        public Entity()
        {
            Errors = new List<string>();
        }

        public bool IsValid()
        {
            return !Errors.Any();
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
        }
    }
}
