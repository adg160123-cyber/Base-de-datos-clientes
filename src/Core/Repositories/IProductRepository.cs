// src/Core/Repositories/IProductRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters; // Para ProductFilter

namespace Utm_market.Core.Repositories;

/// <summary>
/// Define el contrato para el repositorio de productos.
/// Esta interfaz es parte de la capa de Core (Dominio) y solo maneja objetos de dominio (<see cref="Product"/>).
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Recupera todos los productos de forma asíncrona como un flujo de datos.
    /// Utiliza <see cref="IAsyncEnumerable{T}"/> para un manejo eficiente de la memoria
    /// al permitir el streaming de resultados.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que representa un flujo de productos.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    IAsyncEnumerable<Product> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera un producto por su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="productId">El identificador único del producto.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{Product}"/> que contendrá el producto encontrado, o null si no existe.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<Product?> GetByIdAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca productos que coincidan con los criterios especificados en el filtro.
    /// </summary>
    /// <param name="filter">Objeto <see cref="ProductFilter"/> con los criterios de búsqueda.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que representa un flujo de productos que coinciden con el filtro.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    IAsyncEnumerable<Product> FindAsync(ProductFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo producto al repositorio de forma asíncrona.
    /// </summary>
    /// <param name="product">El objeto <see cref="Product"/> a añadir. Su <see cref="Product.ProductID"/> será actualizado
    /// si la base de datos genera un nuevo ID.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task"/> que representa la operación asíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el producto es nulo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un producto existente en el repositorio de forma asíncrona.
    /// </summary>
    /// <param name="product">El objeto <see cref="Product"/> con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si el producto fue encontrado y actualizado, <c>false</c> en caso contrario.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el producto es nulo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza de forma atómica el stock de un producto específico.
    /// </summary>
    /// <param name="productId">El identificador único del producto.</param>
    /// <param name="newStock">La nueva cantidad de stock para el producto. Debe ser no negativa.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{bool}"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si el stock fue actualizado, <c>false</c> si el producto no fue encontrado.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza si <paramref name="newStock"/> es negativo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> UpdateStockAsync(int productId, int newStock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un producto del repositorio de forma asíncrona.
    /// </summary>
    /// <param name="productId">El identificador único del producto a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{bool}"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si el producto fue eliminado, <c>false</c> si no fue encontrado.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default);
}
