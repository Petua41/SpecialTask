using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
	/// <summary>
	/// Контролирует вызов всех команд, их отмену и повтор
	/// Контролирует отложенную инициализацию команд
	/// </summary>
	class CommandsFacade
	{
		Stack<ICommand> stack;
		// TODO
	}

	interface ICommand
	{
		void Execute(params object[] parameters);
	}

	/// <summary>
	/// Представляет команду для редактирования любых атрибутов фигур
	/// </summary>
	class EditShapeAttributesCommand: ICommand
	{
		private Shape reciever;
		private string attribute;
		private object? newValue;

		public EditShapeAttributesCommand(string attribute, Shape reciever)
		{
			this.attribute = attribute;
			this.reciever = reciever;
		}

		public void Execute(params object[] parameters) 
		{
			try { newValue = parameters.Single(); }
			catch (InvalidOperationException) { throw new InvalidNumberOfCommandParametersException(); }
            reciever.Edit(attribute, newValue);
        }
	}

	/// <summary>
	/// Представляет команду для добавления круга на экран
	/// </summary>
	class CreateCircleCommand: ICommand
	{
		private         // TODO: здесь должен быть "контроллер создания фигур"
		private object[]? attributes;

		public CreateCircleCommand()
		{
			// TODO
		}
	}
}
