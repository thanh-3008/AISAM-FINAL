import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/empty_state_widget.dart';
import 'providers/product_controller.dart';
import '../data/models/product_model.dart';

// --- Colors from Tailwind HTML ---
const Color _bgColor = Color(0xFFF7F9FB);
const Color _primaryColor = Color(0xFF003EC7);
const Color _secondaryColor = Color(0xFF6B38D4);
const Color _surfaceContainerLow = Color(0xFFF2F4F6);
const Color _textMuted = Color(0xFF64748B);
const Color _adsOrange = Color(0xFFEA580C);
const Color _publishingPink = Color(0xFFDB2777);
const Color _tertiaryColor = Color(0xFF005851);
const Color _onSurface = Color(0xFF191C1E);
const Color _borderColor = Color.fromRGBO(255, 255, 255, 0.5);

class ProductListScreen extends ConsumerStatefulWidget {
  final String brandId;
  const ProductListScreen({super.key, required this.brandId});

  @override
  ConsumerState<ProductListScreen> createState() => _ProductListScreenState();
}

class _ProductListScreenState extends ConsumerState<ProductListScreen> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final productState = ref.watch(productControllerProvider(widget.brandId));

    return Scaffold(
      backgroundColor: _bgColor,
      floatingActionButton: Container(
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          boxShadow: [
            BoxShadow(
              color: _primaryColor.withValues(alpha: 0.4),
              blurRadius: 24,
              spreadRadius: -6,
              offset: const Offset(0, 12),
            ),
          ],
        ),
        child: FloatingActionButton(
          onPressed: () => context.push('/products/create?brandId=${widget.brandId}'),
          backgroundColor: _primaryColor,
          elevation: 0,
          shape: const CircleBorder(),
          child: const Icon(Icons.add, color: Colors.white, size: 32),
        ),
      ),
      body: CustomScrollView(
        slivers: [
          // Glass App Bar
          SliverAppBar(
            pinned: true,
            expandedHeight: 64.0,
            backgroundColor: Colors.transparent,
            elevation: 0,
            flexibleSpace: ClipRRect(
              child: BackdropFilter(
                filter: ImageFilter.blur(sigmaX: 16.0, sigmaY: 16.0),
                child: Container(
                  decoration: BoxDecoration(
                    color: _bgColor.withValues(alpha: 0.8),
                  ),
                ),
              ),
            ),
            leading: IconButton(
              icon: const Icon(Icons.arrow_back, color: _onSurface),
              onPressed: () {
                if (context.canPop()) {
                  context.pop();
                } else {
                  context.go('/dashboard');
                }
              },
            ),
            title: const Text(
              'Products',
              style: TextStyle(
                fontFamily: 'Plus Jakarta Sans',
                fontWeight: FontWeight.w700,
                fontSize: 24,
                color: _onSurface,
                letterSpacing: -0.01,
              ),
            ),
            centerTitle: false,
            actions: [
              IconButton(
                icon: const Icon(Icons.add, color: _primaryColor, size: 28),
                onPressed: () => context.push('/products/create?brandId=${widget.brandId}'),
              ),
              const SizedBox(width: 8),
            ],
          ),
          
          // Body content
          SliverToBoxAdapter(
            child: productState.when(
              data: (products) {
                final filteredProducts = products.where((p) => 
                  p.name.toLowerCase().contains(_searchQuery.toLowerCase())
                ).toList();

                return Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Search Bar
                      Container(
                        height: 48,
                        decoration: BoxDecoration(
                          color: _surfaceContainerLow,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: TextField(
                          controller: _searchController,
                          onChanged: (value) => setState(() => _searchQuery = value),
                          style: const TextStyle(
                            fontFamily: 'Plus Jakarta Sans',
                            fontSize: 16,
                            color: _onSurface,
                          ),
                          decoration: const InputDecoration(
                            hintText: 'Search products...',
                            hintStyle: TextStyle(
                              color: _textMuted,
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 16,
                            ),
                            prefixIcon: Icon(Icons.search, color: _textMuted),
                            border: InputBorder.none,
                            contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                          ),
                        ),
                      ),
                      const SizedBox(height: 24),
                      
                      // List or Empty
                      if (products.isEmpty)
                        const Padding(
                          padding: EdgeInsets.only(top: 24.0),
                          child: EmptyStateWidget(
                            title: 'Chưa có sản phẩm nào',
                            message: 'Hãy tạo sản phẩm đầu tiên của bạn.',
                            icon: Icons.inventory,
                          ),
                        )
                      else if (filteredProducts.isEmpty)
                         const Padding(
                          padding: EdgeInsets.only(top: 24.0),
                          child: Center(
                            child: Text(
                              'Không tìm thấy sản phẩm nào.',
                              style: TextStyle(color: _textMuted, fontFamily: 'Plus Jakarta Sans'),
                            ),
                          ),
                        )
                      else
                        GridView.builder(
                          padding: EdgeInsets.zero,
                          physics: const NeverScrollableScrollPhysics(),
                          shrinkWrap: true,
                          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                            crossAxisCount: 2,
                            crossAxisSpacing: 16.0,
                            mainAxisSpacing: 16.0,
                            childAspectRatio: 0.82,
                          ),
                          itemCount: filteredProducts.length,
                          itemBuilder: (context, index) {
                            final product = filteredProducts[index];
                            return _ProductGridItem(
                              product: product,
                              index: index,
                            );
                          },
                        ),
                      const SizedBox(height: 80),
                    ],
                  ),
                );
              },
              loading: () => const SizedBox(
                height: 400,
                child: Center(child: AppLoadingIndicator()),
              ),
              error: (error, stack) => SizedBox(
                height: 400,
                child: Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text('Error: $error', textAlign: TextAlign.center),
                      const SizedBox(height: 16),
                      ElevatedButton(
                        onPressed: () => ref.read(productControllerProvider(widget.brandId).notifier).refreshProducts(widget.brandId),
                        child: const Text('Retry'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ProductGridItem extends StatelessWidget {
  final ProductResponseModel product;
  final int index;

  const _ProductGridItem({required this.product, required this.index});

  @override
  Widget build(BuildContext context) {
    final gradients = [
      const LinearGradient(colors: [_primaryColor, _secondaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_primaryColor, _publishingPink], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_tertiaryColor, _primaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_adsOrange, _primaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
    ];
    final avatarGradient = gradients[index % gradients.length];

    return InkWell(
      onTap: () {
        // detail screen
      },
      borderRadius: BorderRadius.circular(20),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(20),
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 12.0, sigmaY: 12.0),
          child: Container(
            decoration: BoxDecoration(
              color: Colors.white.withValues(alpha: 0.6),
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: _borderColor, width: 1.5),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Top half: Avatar/Image area
                Expanded(
                  flex: 3,
                  child: Container(
                    decoration: BoxDecoration(
                      gradient: avatarGradient,
                    ),
                    alignment: Alignment.center,
                    child: product.images != null && product.images!.isNotEmpty
                        ? Image.network(product.images!.first, fit: BoxFit.cover, width: double.infinity, height: double.infinity)
                        : Text(
                            product.name.isNotEmpty ? product.name.substring(0, 1).toUpperCase() : 'P',
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontWeight: FontWeight.w700,
                              fontSize: 48,
                              color: Colors.white,
                            ),
                          ),
                  ),
                ),
                
                // Bottom half: Info area
                Expanded(
                  flex: 2,
                  child: Padding(
                    padding: const EdgeInsets.all(12.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          product.name,
                          style: const TextStyle(
                            fontFamily: 'Plus Jakarta Sans',
                            fontWeight: FontWeight.w700,
                            fontSize: 14,
                            color: _onSurface,
                            letterSpacing: -0.01,
                          ),
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Giá: ${product.price ?? 0}',
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontSize: 12,
                                color: _textMuted,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            const SizedBox(height: 2),
                            Text(
                              'Tồn kho: ${product.stock}',
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontSize: 12,
                                color: _textMuted,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
