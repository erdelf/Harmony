﻿using System.Collections.Generic;
using Harmony.ILCopying;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Collections;

namespace Harmony
{
	public class CodeTranspiler
	{
		private IEnumerable<CodeInstruction> codeInstructions;
		private List<MethodInfo> transpilers = new List<MethodInfo>();

        public CodeTranspiler(List<ILInstruction> ilInstructions) => this.codeInstructions = ilInstructions
                .Select(ilInstruction => ilInstruction.GetCodeInstruction())
                .ToList().AsEnumerable();

        public void Add(MethodInfo transpiler) => this.transpilers.Add(transpiler);

        public static IEnumerable ConvertInstructions(Type type, IEnumerable enumerable)
		{
            Assembly enumerableAssembly = type.GetGenericTypeDefinition().Assembly;
            Type genericListType = enumerableAssembly.GetType(typeof(List<>).FullName);
            Type elementType = type.GetGenericArguments()[0];
            Type listType = enumerableAssembly.GetType(genericListType.MakeGenericType(new Type[] { elementType }).FullName);
            object list = Activator.CreateInstance(listType);
            MethodInfo listAdd = list.GetType().GetMethod("Add");

			foreach (object op in enumerable)
			{
                object elementTo = Activator.CreateInstance(elementType, new object[] { OpCodes.Nop, null });
				Traverse.IterateFields(op, elementTo, (trvFrom, trvDest) => trvDest.SetValue(trvFrom.GetValue()));
				listAdd.Invoke(list, new object[] { elementTo });
			}
			return list as IEnumerable;
		}

		public static IEnumerable ConvertInstructions(MethodInfo transpiler, IEnumerable enumerable)
		{
            Type type = transpiler.GetParameters()
				  .Select(p => p.ParameterType)
				  .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("IEnumerable"));
			return ConvertInstructions(type, enumerable);
		}

		public IEnumerable<CodeInstruction> GetResult(ILGenerator generator, MethodBase method)
		{
			IEnumerable instructions = this.codeInstructions;
            this.transpilers.ForEach(transpiler =>
			{
				instructions = ConvertInstructions(transpiler, instructions);
                List<object> parameter = new List<object>();
				transpiler.GetParameters().Select(param => param.ParameterType).Do(type =>
				{
					if (type.IsAssignableFrom(typeof(ILGenerator)))
						parameter.Add(generator);
					else if (type.IsAssignableFrom(typeof(MethodBase)))
						parameter.Add(method);
					else
						parameter.Add(instructions);
				});
				instructions = transpiler.Invoke(null, parameter.ToArray()) as IEnumerable;
			});
			instructions = ConvertInstructions(typeof(IEnumerable<CodeInstruction>), instructions);
			return instructions as IEnumerable<CodeInstruction>;
		}
	}
}