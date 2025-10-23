// ----------------------------------------------------------------------
// File: 		IGenericEventArgs
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2014 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

/// <summary>
///
/// </summary>
public interface IGenericEventArgs<T>
{
    #region PROPERTIES

    T Value { get; }

    #endregion PROPERTIES
}