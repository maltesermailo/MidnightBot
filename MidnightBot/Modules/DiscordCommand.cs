﻿using Discord.Commands;
using MidnightBot.Modules;

namespace MidnightBot.Classes
{
    /// <summary>
    /// Base DiscordCommand Class.
    /// Inherit this class to create your own command.
    /// </summary>
    public abstract class DiscordCommand
    {
        /// <summary>
        /// Parent module
        /// </summary>
        protected DiscordModule Module { get; }

        /// Parent module's prefix
        /// </summary>
        protected string Prefix => Module.Prefix;

        /// <summary>
        /// <summary>
        /// Creates a new instance of discord command,
        /// use ": base(module)" in the derived class'
        /// constructor to make sure module is assigned
        /// </summary>
        /// <param name="module">Module this command resides in</param>
        protected DiscordCommand ( DiscordModule module )
        {
            this.Module = module;
        }

        /// <summary>
        /// Initializes the CommandBuilder with values using CommandGroupBuilder
        /// </summary>
        internal abstract void Init(CommandGroupBuilder cgb);
    }
}
