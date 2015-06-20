namespace OrigoDB.Core
{

    /// <summary>
    /// An engine executes commands and queries
    /// </summary>
	public interface IEngine<TModel> where TModel : Model
    {

        /// <summary>
        /// Execute a query
        /// </summary>
        TResult Execute<TResult>(Query<TModel, TResult> query);
        
        /// <summary>
        /// Execute a command with no result
        /// </summary>
        void Execute(Command<TModel> command);

        /// <summary>
        /// Execute a command that returns results
        /// </summary>
        TResult Execute<TResult>(Command<TModel, TResult> command);

        object Execute(Command command);

        object Execute(Query query);
    }
}
