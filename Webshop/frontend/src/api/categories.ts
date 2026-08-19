import { http } from "./http"
import type { CategoryDto, ItemResult, SearchResult } from "@/types/models"

export async function fetchCategories(): Promise<CategoryDto[]> {
  const { data } = await http.get<SearchResult<CategoryDto>>("/categories/search", {
    params: { pageSize: 0 },
  })
  return data.items
}

export async function fetchCategory(id: number): Promise<CategoryDto> {
  const { data } = await http.get<ItemResult<CategoryDto>>(`/categories/${id}`)
  return data.item
}
