export enum AdType {
  TextOnly = 0,
  ImageText = 1,
  VideoText = 2,
}

export interface ConversationResponseDto {
  id: string;
  profileId: string;
  brandId?: string | null;
  brandName?: string | null;
  productId?: string | null;
  productName?: string | null;
  adType: AdType;
  title?: string | null;
  isActive: boolean;
  lastMessage?: string | null;
  lastMessageAt?: string | null;
  messageCount: number;
}

export interface ChatMessageDto {
  id: string;
  senderType: number;
  message: string;
  aiGenerationId?: string | null;
  contentId?: string | null;
  createdAt: string;
}

export interface ConversationDetailDto extends ConversationResponseDto {
  messages: ChatMessageDto[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
