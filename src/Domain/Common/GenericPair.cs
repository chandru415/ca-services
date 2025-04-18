namespace Domain.Common
{
    public record GenericPair<T, U>
    {
        public required T Key { get; set; }
        public U? Value { get; set; }
    }
}
