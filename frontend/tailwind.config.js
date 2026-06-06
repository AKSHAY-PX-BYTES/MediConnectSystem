/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  darkMode: "class",
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
      colors: {
        brand: {
          50:  "#eff6ff",
          100: "#dbeafe",
          200: "#bfdbfe",
          300: "#93c5fd",
          400: "#60a5fa",
          500: "#3b82f6",
          600: "#2563eb",
          700: "#1d4ed8",
          800: "#1e40af",
          900: "#1e3a8a",
          950: "#172554",
        }
      },
      animation: {
        'fade-in':  'mcFadeIn 0.4s ease-out both',
        'slide-up': 'mcSlideUp 0.4s ease-out both',
        'spin-slow': 'mcSpin 0.8s linear infinite',
      },
      keyframes: {
        mcFadeIn:  { from: { opacity: '0' },                           to: { opacity: '1' } },
        mcSlideUp: { from: { opacity: '0', transform: 'translateY(16px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        mcSpin:    { to:   { transform: 'rotate(360deg)' } },
      },
    }
  },
  // Disable preflight so Angular Material's base styles are not overridden.
  // We apply our own minimal reset in styles.css instead.
  corePlugins: {
    preflight: false
  },
  plugins: []
};
