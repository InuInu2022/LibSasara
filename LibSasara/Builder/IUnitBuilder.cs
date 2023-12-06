using System;
using LibSasara.Model;

namespace LibSasara.Builder;

/// <summary>
/// <see cref="UnitBase"/>を作成するBuilderクラス
/// </summary>
/// <typeparam name="TUnit"></typeparam>
/// <typeparam name="TBuilder"></typeparam>
internal interface IUnitBuilder<TUnit,TBuilder>
	where TUnit: UnitBase
	where TBuilder: IUnitBuilder<TUnit,TBuilder>
{
	/// <inheritdoc cref="UnitBase.Group" path="/summary"/>
    /// <seealso cref="UnitBase.Group"/>
	TBuilder Group(Guid guid);

	/// <summary>
	/// <see cref="UnitBase"/>を作成
	/// </summary>
	/// <param name="canAdd">生成と同時にccs/ccstに追加する</param>
	/// <returns>作成した<see cref="UnitBase"/>を返す</returns>
    /// <seealso cref="UnitBase"/>
	TUnit Build(bool canAdd = true);
}
