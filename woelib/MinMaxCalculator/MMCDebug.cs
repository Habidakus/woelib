namespace woelib.MinMaxCalculator
{
    /// <summary>
    /// Class used to help debug usage of the [MinMaxCalculator]
    /// 
    /// If you're having trouble determining why one action was chosen over another, and suspect there's
    /// a bug with your score evaluation code, you can give an instance of [MMCDebug] to the [method MinMaxCalculator.get_best_action]
    /// function and then invoke the [method dump] function to have all the considered actions and game states printed out.
    /// </summary>
    public class MMCDebug
    {
        Dictionary<IMMCGameState, Dictionary<IMMCAction, Tuple<IMMCScore, IMMCGameState>?>> _allGameStates = new();

        internal void AddActions(IMMCGameState gameState, IMMCAction[] actions)
        {
            if (_allGameStates.ContainsKey(gameState))
            {
                throw new Exception("Code failure: game state already registered with this MMCDebug instance");
            }

            Dictionary<IMMCAction, Tuple<IMMCScore, IMMCGameState>?> gsActions = new();
            foreach (IMMCAction action in actions)
            {
                gsActions[action] = null;
            }

            _allGameStates[gameState] = gsActions;
        }

        internal void AddResult(IMMCGameState gameState, IMMCAction action, IMMCGameState resultState, IMMCScore score)
        {
            if (_allGameStates.ContainsKey(gameState))
            {
                if (_allGameStates[gameState].ContainsKey(action))
                {
                    _allGameStates[gameState][action] = Tuple.Create(score, resultState);
                }
                else
                {
                    Console.WriteLine($"MMCDebug[{gameState}] has no action: {action}");
                }
            }
            else
            {
                Console.WriteLine($"NO GAME STATE FOUND in MMCDebug: {gameState}");
            }
        }

        private static string Indent(int index)
        {
            return new string(' ', index);
        }

        /// <summary>
        /// Will invoke the print() function for each action/game_state pair in the evaluation tree. Note
        /// that your implementations of game state and action should also implement _to_string() in order
        /// to be useful when the dump() method is called.
        /// </summary>
        public void Dump(IMMCGameState gameState)
        {
            Console.WriteLine(gameState.ToString());

            if(_allGameStates.TryGetValue(gameState, out Dictionary<IMMCAction, Tuple<IMMCScore, IMMCGameState>?>? actionDict) == false)
            {
                Console.WriteLine(Indent(1) + "GAME STATE NOT FOUND");
                return;
            }

            if (!actionDict.Any())
            {
                Console.WriteLine(Indent(1) + "NO ACTIONS");
                return;
            }

            foreach ((IMMCAction action, Tuple<IMMCScore, IMMCGameState>? tuple) in actionDict)
            {
                if (tuple == null)
                {
                    Console.WriteLine($"{Indent(1)}{action} : not evaluated");
                }
                else
                {
                    Console.WriteLine($"{Indent(1)}{action} vvv");
                    DumpInternal(2, tuple.Item1, tuple.Item2);
                }
            }
        }

        private void DumpInternal(int ind, IMMCScore score, IMMCGameState gameState)
        {
            Console.WriteLine($"{Indent(ind)}{gameState}");

            if (_allGameStates.TryGetValue(gameState, out Dictionary<IMMCAction, Tuple<IMMCScore, IMMCGameState>?>? actionDict) == false)
            {
                Console.WriteLine(Indent(ind + 1) + "GAME STATE NOT FOUND");
                return;
            }

            if (!actionDict.Any())
            {
                Console.WriteLine(Indent(ind + 1) + "NO ACTIONS");
                return;
            }

            foreach ((IMMCAction action, Tuple<IMMCScore, IMMCGameState>? tuple) in actionDict)
            {
                if (tuple == null)
                {
                    Console.WriteLine($"{Indent(ind+1)}{action} : not evaluated");
                }
                else
                {
                    Console.WriteLine($"{Indent(ind+1)}{action} vvv");
                    DumpInternal(ind+2, tuple.Item1, tuple.Item2);
                }
            }
        }

    }
}
