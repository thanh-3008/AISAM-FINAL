import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/auth_controller.dart';
import '../../../core/state/base_state.dart';
import '../data/models/auth_response.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _onLogin() {
    if (_formKey.currentState!.validate()) {
      ref.read(authControllerProvider.notifier).login(
            _emailController.text.trim(),
            _passwordController.text,
          );
    }
  }

  void _onGoogleLogin() {
    // Tạm thời mock Google Login
    ref.read(authControllerProvider.notifier).googleLogin('mocked-google-id-token');
  }

  Widget _buildSocialButton({
    required String iconUrl,
    required String label,
    required VoidCallback onTap,
  }) {
    return Expanded(
      child: ElevatedButton(
        onPressed: onTap,
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color(0xFF1A73E8),
          foregroundColor: Colors.white,
          elevation: 0,
          padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(24),
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(6),
              decoration: const BoxDecoration(
                color: Colors.white,
                shape: BoxShape.circle,
              ),
              child: Image.network(
                iconUrl,
                height: 20,
                width: 20,
                errorBuilder: (context, error, stackTrace) {
                  return const Icon(Icons.public, size: 20, color: Colors.grey);
                },
              ),
            ),
            const SizedBox(width: 12),
            Text(
              label,
              style: GoogleFonts.plusJakartaSans(
                fontSize: 15,
                fontWeight: FontWeight.w600,
                color: Colors.white,
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authControllerProvider);
    final isLoading = authState.maybeWhen(
      loading: () => true,
      orElse: () => false,
    );

    ref.listen<BaseState<AuthResponseModel>>(authControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) {
          AppSnackbar.showError(context, error.toString());
        },
        data: (data) {
          // Success, Router will redirect automatically because token is saved, but we can force refresh
          context.go('/dashboard'); 
        },
        orElse: () {},
      );
    });

    return Scaffold(
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(
          color: Color(0xFFF7F9FB),
        ),
        child: Stack(
          children: [
            // Top-left soft gradient circle
            Positioned(
              top: -150,
              left: -150,
              child: Container(
                width: 400,
                height: 400,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: const Color(0xFFDDE1FF).withOpacity(0.35),
                ),
              ),
            ),
            // Bottom-right soft gradient circle
            Positioned(
              bottom: -150,
              right: -150,
              child: Container(
                width: 400,
                height: 400,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: const Color(0xFFD2E0FE).withOpacity(0.35),
                ),
              ),
            ),
            // Backdrop filter for smooth background blur
            Positioned.fill(
              child: BackdropFilter(
                filter: ImageFilter.blur(sigmaX: 80, sigmaY: 80),
                child: const SizedBox(),
              ),
            ),
            // Main content
            SafeArea(
              child: LayoutBuilder(
                builder: (context, constraints) {
                  return SingleChildScrollView(
                    child: ConstrainedBox(
                      constraints: BoxConstraints(
                        minHeight: constraints.maxHeight,
                      ),
                      child: IntrinsicHeight(
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 20),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              const SizedBox(height: 32),
                              // Branding Header Section
                              Center(
                                child: Column(
                                  children: [
                                    Container(
                                      width: 64,
                                      height: 64,
                                      decoration: BoxDecoration(
                                        color: const Color(0xFF0052FF),
                                        borderRadius: BorderRadius.circular(16),
                                        boxShadow: [
                                          BoxShadow(
                                            color: const Color(0xFF0052FF).withOpacity(0.3),
                                            blurRadius: 16,
                                            offset: const Offset(0, 8),
                                          ),
                                        ],
                                      ),
                                      child: const Icon(
                                        Icons.auto_awesome,
                                        color: Color(0xFFDFE3FF),
                                        size: 36,
                                      ),
                                    ),
                                    const SizedBox(height: 16),
                                    Text(
                                      'AISAM',
                                      style: GoogleFonts.plusJakartaSans(
                                        fontSize: 32,
                                        fontWeight: FontWeight.w800,
                                        color: const Color(0xFF0A192F),
                                        letterSpacing: -0.5,
                                      ),
                                    ),
                                    const SizedBox(height: 4),
                                    Text(
                                      'Intelligence at your fingertips.',
                                      style: GoogleFonts.plusJakartaSans(
                                        fontSize: 16,
                                        fontWeight: FontWeight.w500,
                                        color: const Color(0xFF515F78),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 32),
                              // Glassmorphic Card
                              ClipRRect(
                                borderRadius: BorderRadius.circular(32),
                                child: BackdropFilter(
                                  filter: ImageFilter.blur(sigmaX: 20, sigmaY: 20),
                                  child: Container(
                                    padding: const EdgeInsets.all(24),
                                    decoration: BoxDecoration(
                                      color: Colors.white.withOpacity(0.65),
                                      borderRadius: BorderRadius.circular(32),
                                      border: Border.all(
                                        color: Colors.white.withOpacity(0.5),
                                        width: 1.5,
                                      ),
                                      boxShadow: [
                                        BoxShadow(
                                          color: const Color(0xFF003EC7).withOpacity(0.06),
                                          blurRadius: 40,
                                          offset: const Offset(0, 20),
                                        ),
                                      ],
                                    ),
                                    child: Form(
                                      key: _formKey,
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: [
                                          // Email Input label
                                          Text(
                                            'Email Address',
                                            style: GoogleFonts.plusJakartaSans(
                                              fontSize: 14,
                                              fontWeight: FontWeight.w600,
                                              color: const Color(0xFF434656),
                                            ),
                                          ),
                                          const SizedBox(height: 8),
                                          TextFormField(
                                            controller: _emailController,
                                            keyboardType: TextInputType.emailAddress,
                                            style: GoogleFonts.plusJakartaSans(
                                              fontSize: 16,
                                              color: const Color(0xFF191C1E),
                                            ),
                                            decoration: InputDecoration(
                                              hintText: 'name@company.ai',
                                              hintStyle: GoogleFonts.plusJakartaSans(
                                                fontSize: 15,
                                                color: const Color(0xFF737688).withOpacity(0.5),
                                              ),
                                              prefixIcon: const Icon(
                                                Icons.mail_outline,
                                                color: Color(0xFF737688),
                                              ),
                                              filled: true,
                                              fillColor: const Color(0xFFF2F4F6),
                                              contentPadding: const EdgeInsets.symmetric(vertical: 16, horizontal: 16),
                                              border: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: BorderSide.none,
                                              ),
                                              enabledBorder: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: BorderSide(
                                                  color: const Color(0xFFC3C5D9).withOpacity(0.4),
                                                ),
                                              ),
                                              focusedBorder: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: const BorderSide(
                                                  color: Color(0xFF003EC7),
                                                  width: 1.5,
                                                ),
                                              ),
                                              errorBorder: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: const BorderSide(
                                                  color: Colors.red,
                                                  width: 1,
                                                ),
                                              ),
                                            ),
                                            validator: (value) {
                                              if (value == null || value.isEmpty) return 'Please enter email';
                                              if (!value.contains('@')) return 'Invalid email';
                                              return null;
                                            },
                                          ),
                                          const SizedBox(height: 16),
                                          // Password Label & Forgot link
                                          Row(
                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                            children: [
                                              Text(
                                                'Password',
                                                style: GoogleFonts.plusJakartaSans(
                                                  fontSize: 14,
                                                  fontWeight: FontWeight.w600,
                                                  color: const Color(0xFF434656),
                                                ),
                                              ),
                                              GestureDetector(
                                                onTap: () => context.push('/forgot-password'),
                                                child: Text(
                                                  'Forgot?',
                                                  style: GoogleFonts.plusJakartaSans(
                                                    fontSize: 14,
                                                    fontWeight: FontWeight.w600,
                                                    color: const Color(0xFF003EC7),
                                                  ),
                                                ),
                                              ),
                                            ],
                                          ),
                                          const SizedBox(height: 8),
                                          TextFormField(
                                            controller: _passwordController,
                                            obscureText: _obscurePassword,
                                            style: GoogleFonts.plusJakartaSans(
                                              fontSize: 16,
                                              color: const Color(0xFF191C1E),
                                            ),
                                            decoration: InputDecoration(
                                              hintText: '••••••••',
                                              hintStyle: GoogleFonts.plusJakartaSans(
                                                fontSize: 15,
                                                color: const Color(0xFF737688).withOpacity(0.5),
                                              ),
                                              prefixIcon: const Icon(
                                                Icons.lock_outline,
                                                color: Color(0xFF737688),
                                              ),
                                              suffixIcon: IconButton(
                                                icon: Icon(
                                                  _obscurePassword ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                                                  color: const Color(0xFF737688),
                                                ),
                                                onPressed: () {
                                                  setState(() {
                                                    _obscurePassword = !_obscurePassword;
                                                  });
                                                },
                                              ),
                                              filled: true,
                                              fillColor: const Color(0xFFF2F4F6),
                                              contentPadding: const EdgeInsets.symmetric(vertical: 16, horizontal: 16),
                                              border: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: BorderSide.none,
                                              ),
                                              enabledBorder: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: BorderSide(
                                                  color: const Color(0xFFC3C5D9).withOpacity(0.4),
                                                ),
                                              ),
                                              focusedBorder: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: const BorderSide(
                                                  color: Color(0xFF003EC7),
                                                  width: 1.5,
                                                ),
                                              ),
                                              errorBorder: OutlineInputBorder(
                                                borderRadius: BorderRadius.circular(16),
                                                borderSide: const BorderSide(
                                                  color: Colors.red,
                                                  width: 1,
                                                ),
                                              ),
                                            ),
                                            validator: (value) {
                                              if (value == null || value.isEmpty) return 'Please enter password';
                                              return null;
                                            },
                                          ),
                                          const SizedBox(height: 24),
                                          // Sign In button
                                          ElevatedButton(
                                            onPressed: isLoading ? null : _onLogin,
                                            style: ElevatedButton.styleFrom(
                                              backgroundColor: const Color(0xFF003EC7),
                                              foregroundColor: Colors.white,
                                              disabledBackgroundColor: const Color(0xFF003EC7).withOpacity(0.6),
                                              minimumSize: const Size.fromHeight(56),
                                              elevation: 4,
                                              shadowColor: const Color(0xFF003EC7).withOpacity(0.4),
                                              shape: RoundedRectangleBorder(
                                                borderRadius: BorderRadius.circular(28),
                                              ),
                                            ),
                                            child: isLoading
                                                ? const SizedBox(
                                                    width: 24,
                                                    height: 24,
                                                    child: CircularProgressIndicator(
                                                      color: Colors.white,
                                                      strokeWidth: 2,
                                                    ),
                                                  )
                                                : Text(
                                                    'Sign In',
                                                    style: GoogleFonts.plusJakartaSans(
                                                      fontSize: 18,
                                                      fontWeight: FontWeight.w600,
                                                    ),
                                                  ),
                                          ),
                                          const SizedBox(height: 24),
                                          // Divider
                                          Row(
                                            children: [
                                              const Expanded(
                                                child: Divider(
                                                  color: Color(0xFFC3C5D9),
                                                  thickness: 1,
                                                ),
                                              ),
                                              Padding(
                                                padding: const EdgeInsets.symmetric(horizontal: 12),
                                                child: Text(
                                                  'Or continue with',
                                                  style: GoogleFonts.plusJakartaSans(
                                                    fontSize: 12,
                                                    fontWeight: FontWeight.w500,
                                                    color: const Color(0xFF737688),
                                                  ),
                                                ),
                                              ),
                                              const Expanded(
                                                child: Divider(
                                                  color: Color(0xFFC3C5D9),
                                                  thickness: 1,
                                                ),
                                              ),
                                            ],
                                          ),
                                          const SizedBox(height: 24),
                                          // Social Logins
                                          Row(
                                            children: [
                                              _buildSocialButton(
                                                iconUrl: 'https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Google_%22G%22_logo.svg/120px-Google_%22G%22_logo.svg.png',
                                                label: 'Google',
                                                onTap: _onGoogleLogin,
                                              ),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                  ),
                                ),
                              ),
                              const SizedBox(height: 24),
                              // Footer registration text
                              Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Text(
                                    'New to AISAM? ',
                                    style: GoogleFonts.plusJakartaSans(
                                      fontSize: 16,
                                      color: const Color(0xFF515F78),
                                    ),
                                  ),
                                  GestureDetector(
                                    onTap: () => context.push('/register'),
                                    child: Text(
                                      'Create Account',
                                      style: GoogleFonts.plusJakartaSans(
                                        fontSize: 16,
                                        fontWeight: FontWeight.bold,
                                        color: const Color(0xFF003EC7),
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                              const Spacer(),
                              // Privacy Policy & Terms of Service links at the bottom
                              Padding(
                                padding: const EdgeInsets.symmetric(vertical: 24),
                                child: Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    GestureDetector(
                                      onTap: () {
                                        AppSnackbar.showError(context, 'Privacy Policy clicked.');
                                      },
                                      child: Text(
                                        'Privacy Policy',
                                        style: GoogleFonts.plusJakartaSans(
                                          fontSize: 12,
                                          color: const Color(0xFF737688),
                                          fontWeight: FontWeight.w500,
                                        ),
                                      ),
                                    ),
                                    const SizedBox(width: 24),
                                    GestureDetector(
                                      onTap: () {
                                        AppSnackbar.showError(context, 'Terms of Service clicked.');
                                      },
                                      child: Text(
                                        'Terms of Service',
                                        style: GoogleFonts.plusJakartaSans(
                                          fontSize: 12,
                                          color: const Color(0xFF737688),
                                          fontWeight: FontWeight.w500,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

