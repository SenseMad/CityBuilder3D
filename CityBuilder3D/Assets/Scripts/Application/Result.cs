namespace Scripts.Application
{
  public readonly struct Result
  {
    public bool IsSuccess { get; }
    public string Error { get; }

    private Result(bool isSuccess, string error)
    {
      IsSuccess = isSuccess;
      Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string error) => new(false, error);
  }

  public readonly struct Result<T>
  {
    public bool IsSuccess { get; }
    public string Error { get; }
    public T Value { get; }

    private Result(bool isSuccess, T value, string error)
    {
      IsSuccess = isSuccess;
      Value = value;
      Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default!, error);
  }
}