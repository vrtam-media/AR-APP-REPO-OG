// ----------------------------------------------------------------------
// File: 			BevelBuilder
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2020 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Virtence.VText
{
	/// <summary>
	/// Strategy context for bevel building
	/// </summary>
	internal class BevelBuilder : BevelBuilderStrategy
	{
		#region CONSTANTS

		//

		#endregion // CONSTANTS



		#region FIELDS
		private BevelStyle _style;
		private BevelBuilderStrategy _strategy;
		#endregion // FIELDS



		#region PROPERTIES
		public BevelStyle Style
		{
			get
			{
				return _style;
			}
			set
			{
				if(value != _style || null == _strategy)
				{
					_style = value;
					switch(_style)
					{
						case BevelStyle.Chiseled:
							_strategy = new BevelBuilderChiseled(_meshParameter);
							break;
						case BevelStyle.Flat:
							_strategy = new BevelBuilderFlat(_meshParameter);
							break;
						case BevelStyle.Profile:
							_strategy = new BevelBuilderProfile(_meshParameter);
							break;
						case BevelStyle.Round:
							_strategy = new BevelBuilderRound(_meshParameter);
							break;
						case BevelStyle.Step:
							_strategy = new BevelBuilderStep(_meshParameter);
							break;
						default: throw new ArgumentException("Unknown bevel style.");
					}
				}
			}
		}
		#endregion // PROPERTIES



		#region CONSTRUCTORS
		internal BevelBuilder(VTextMeshParameter meshParameter) : base(meshParameter)
		{
			Style = meshParameter.BevelStyle;
		}

		#endregion // CONSTRUCTORS



		#region METHODS

		internal override void AddBevelFrontFacesToMesh(ref MeshAttributes meshAttribs)
		{
			_strategy.AddBevelFrontFacesToMesh(ref meshAttribs);
		}

		internal override void AddBevelBackfacesToMesh(ref MeshAttributes meshAttribs)
		{
			_strategy.AddBevelBackfacesToMesh(ref meshAttribs);
		}

		#endregion // METHODS
	}
}
