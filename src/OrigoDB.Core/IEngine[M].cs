namespace OrigoDB.Core
{

    /// <summary>
    /// An engine executes commands and queries
    /// </summary>
	public interface IEngine<TModel> where TModel : Model
    {
        TResult Execute<TResult>(Query<TModel, TResult> query);
        
        
        void Execute(Command<TModel> command);

        /// <summary>
        /// Execute a command that returns results
        /// </summary>
        TResult Execute<TResult>(Command<TModel, TResult> command);
    }
}
