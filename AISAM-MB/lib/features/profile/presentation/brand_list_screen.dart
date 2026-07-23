import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/empty_state_widget.dart';
import 'providers/brand_controller.dart';
import '../data/models/brand_model.dart';

// --- Colors from Tailwind HTML ---
const Color _bgColor = Color(0xFFF7F9FB);
const Color _primaryColor = Color(0xFF003EC7);
const Color _secondaryColor = Color(0xFF6B38D4);
const Color _surfaceContainerLow = Color(0xFFF2F4F6);
const Color _surfaceContainer = Color(0xFFECEEF0);
const Color _textMuted = Color(0xFF64748B);
const Color _adsOrange = Color(0xFFEA580C);
const Color _publishingPink = Color(0xFFDB2777);
const Color _tertiaryColor = Color(0xFF005851);
const Color _onSurface = Color(0xFF191C1E);
const Color _borderColor = Color.fromRGBO(255, 255, 255, 0.5);
const Color _activeGreen = Color(0xFF22C55E); // green-500 equivalent

class BrandListScreen extends ConsumerStatefulWidget {
  const BrandListScreen({super.key});

  @override
  ConsumerState<BrandListScreen> createState() => _BrandListScreenState();
}

class _BrandListScreenState extends ConsumerState<BrandListScreen> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final brandState = ref.watch(brandControllerProvider);

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
          onPressed: () => context.push('/brands/create'),
          backgroundColor: _primaryColor,
          elevation: 0, // Managed by container shadow
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
                  context.go('/dashboard'); // fallback
                }
              },
            ),
            title: const Text(
              'Brands',
              style: TextStyle(
                fontFamily: 'Plus Jakarta Sans',
                fontWeight: FontWeight.w700,
                fontSize: 24, // headline-md
                color: _onSurface,
                letterSpacing: -0.01,
              ),
            ),
            centerTitle: false,
            actions: [
              IconButton(
                icon: const Icon(Icons.add, color: _primaryColor, size: 28),
                onPressed: () => context.push('/brands/create'),
              ),
              const SizedBox(width: 8),
            ],
          ),
          
          // Body content
          SliverToBoxAdapter(
            child: brandState.when(
              data: (brands) {
                // Filter brands
                final filteredBrands = brands.where((b) => 
                  b.name.toLowerCase().contains(_searchQuery.toLowerCase())
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
                            hintText: 'Search your brands...',
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
                      if (brands.isEmpty)
                        const Padding(
                          padding: EdgeInsets.only(top: 24.0),
                          child: EmptyStateWidget(
                            title: 'No Brands Found',
                            message: 'Let\'s create your first brand.',
                            icon: Icons.branding_watermark,
                          ),
                        )
                      else if (filteredBrands.isEmpty)
                         const Padding(
                          padding: EdgeInsets.only(top: 24.0),
                          child: Center(
                            child: Text(
                              'No brands match your search.',
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
                            childAspectRatio: 0.82, // adjust ratio so image and text fit nicely
                          ),
                          itemCount: filteredBrands.length,
                          itemBuilder: (context, index) {
                            final brand = filteredBrands[index];
                            return _BrandGridItem(
                              brand: brand,
                              index: index,
                            );
                          },
                        ),
                      const SizedBox(height: 80), // padding for FAB
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
                        onPressed: () => ref.read(brandControllerProvider.notifier).refreshBrands(),
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

class _BrandGridItem extends ConsumerWidget {
  final BrandResponseModel brand;
  final int index;

  const _BrandGridItem({required this.brand, required this.index});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    String placeholderDesc = brand.slogan ?? brand.description ?? '';
    if (placeholderDesc.isEmpty) {
      placeholderDesc = brand.name.toLowerCase().contains('shoe') 
          ? 'Cửa hàng Giày Thể Thao' 
          : 'Thương Hiệu & Dịch Vụ';
    }

    // Default status to Active for now as per mockups
    final bool isActive = true; // In a real scenario, use brand.status

    final gradients = [
      const LinearGradient(colors: [_primaryColor, _secondaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_primaryColor, _publishingPink], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_tertiaryColor, _primaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_adsOrange, _primaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
    ];
    final bgGradient = gradients[index % gradients.length];

    return ClipRRect(
      borderRadius: BorderRadius.circular(12),
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 10.0, sigmaY: 10.0),
        child: Container(
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: 0.7),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: _borderColor),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: 0.04),
                blurRadius: 24,
                offset: const Offset(0, 8),
              ),
            ],
          ),
          child: Material(
            color: Colors.transparent,
            child: InkWell(
              onTap: () {
                context.push('/brands/${brand.id}/products');
              },
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // Top Image Area
                  SizedBox(
                    height: 100,
                    child: Stack(
                      fit: StackFit.expand,
                      children: [
                        // Image or Gradient
                        brand.logoUrl != null && brand.logoUrl!.isNotEmpty
                            ? Image.network(
                                brand.logoUrl!,
                                fit: BoxFit.cover,
                                errorBuilder: (context, error, stackTrace) => 
                                    Container(decoration: BoxDecoration(gradient: bgGradient)),
                              )
                            : Container(
                                decoration: BoxDecoration(gradient: bgGradient),
                                child: Center(
                                  child: Text(
                                    brand.name.isNotEmpty ? brand.name.substring(0, 1).toUpperCase() : 'B',
                                    style: const TextStyle(
                                      fontFamily: 'Plus Jakarta Sans',
                                      fontSize: 32,
                                      fontWeight: FontWeight.bold,
                                      color: Colors.white,
                                    ),
                                  ),
                                ),
                              ),
                        
                        // Status Pill
                        Positioned(
                          top: 8,
                          right: 8,
                          child: Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: _activeGreen,
                              borderRadius: BorderRadius.circular(9999),
                            ),
                            child: const Text(
                              'ACTIVE',
                              style: TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontSize: 10,
                                fontWeight: FontWeight.w700,
                                color: Colors.white,
                                letterSpacing: 0.05, // tracking-wider
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  
                  // Bottom Info Area
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.all(12.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            brand.name,
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontWeight: FontWeight.w600,
                              fontSize: 14, // label-md
                              color: _onSurface,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 4),
                          Text(
                            placeholderDesc,
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 11,
                              color: _textMuted,
                            ),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const Spacer(),
                          // Social Icons placeholder
                          Row(
                            children: [
                              Container(
                                width: 24,
                                height: 24,
                                decoration: const BoxDecoration(
                                  color: _surfaceContainer,
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(Icons.facebook, size: 14, color: _textMuted),
                              ),
                              const SizedBox(width: 8),
                              Container(
                                width: 24,
                                height: 24,
                                decoration: const BoxDecoration(
                                  color: _surfaceContainer,
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(Icons.camera_alt, size: 14, color: _textMuted), // IG placeholder
                              ),
                            ],
                          )
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

