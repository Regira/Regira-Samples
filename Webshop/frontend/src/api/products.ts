import { http } from "./http"
import type { ItemResult, ProductDto, SearchResult } from "@/types/models"

export type ProductSortBy = "Default" | "Title" | "TitleDesc" | "Price" | "PriceDesc" | "Rating" | "Newest"

export interface ProductSearchParams {
  q?: string
  categoryId?: number[]
  brand?: string[]
  minPrice?: number
  maxPrice?: number
  inStockOnly?: boolean
  isFeatured?: boolean
  onSale?: boolean
  slug?: string
  page?: number
  pageSize?: number
  sortBy?: ProductSortBy | ProductSortBy[]
}

export async function searchProducts(params: ProductSearchParams): Promise<SearchResult<ProductDto>> {
  const { data } = await http.get<SearchResult<ProductDto>>("/products/search", { params })
  return data
}

export async function fetchProduct(id: number): Promise<ProductDto> {
  const { data } = await http.get<ItemResult<ProductDto>>(`/products/${id}`)
  return data.item
}

export async function fetchProductBySlug(slug: string): Promise<ProductDto | null> {
  const { items } = await searchProducts({ slug, pageSize: 1 })
  return items[0] ?? null
}

export async function fetchRelatedProducts(categoryId: number, excludeId: number, take = 4): Promise<ProductDto[]> {
  const { data } = await http.get<SearchResult<ProductDto>>("/products/search", {
    params: { categoryId: [categoryId], exclude: [excludeId], pageSize: take, sortBy: "Rating" },
  })
  return data.items
}
