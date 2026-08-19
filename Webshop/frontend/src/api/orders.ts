import { http } from "./http"
import type { FieldErrors, ItemResult, OrderDto, OrderInputDto } from "@/types/models"

export class OrderValidationError extends Error {
  constructor(
    message: string,
    public errors: FieldErrors,
  ) {
    super(message)
  }
}

export async function createOrder(input: OrderInputDto): Promise<OrderDto> {
  try {
    const { data } = await http.post<ItemResult<OrderDto>>("/orders", input)
    return data.item
  } catch (err: unknown) {
    const axiosErr = err as { response?: { status?: number; data?: { errors?: FieldErrors; title?: string } } }
    if (axiosErr.response?.status === 400) {
      throw new OrderValidationError(axiosErr.response.data?.title ?? "Could not place order", axiosErr.response.data?.errors ?? {})
    }
    throw err
  }
}

export async function fetchOrder(id: number): Promise<OrderDto> {
  const { data } = await http.get<ItemResult<OrderDto>>(`/orders/${id}`, { params: { includes: "All" } })
  return data.item
}
