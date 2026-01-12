import { motion } from 'framer-motion';
import {
  Play,
  ArrowRight,
} from 'lucide-react';
import { Button } from './Button';
import { StepCard } from './StepCard';
import { ImageWithFallback } from './ImageWithFallback';
import "./index.css"
import { useNavigate } from 'react-router-dom';
import { Header } from '../layouts/Header';
import {useSelector} from "react-redux";



export const LandingPage = () => {
    const navigate = useNavigate();
    const { token } = useSelector((state: any) => state.auth);

  const steps = [
    {
      title: "Привяжите экраны",
      description: "Введите специальный код, отображаемый при запуске приложения."
    },
    {
      title: "Загрузите контент",
      description: "Добавьте видео, изображения или создайте таблицу, создавая конфигурацию экрана через удобный интерфейс."
    },
    {
      title: "Запустите трансляцию",
      description: "Нажмите кнопку и контент появится на всех выбранных экранах."
    }
  ];


  return (
    <div className="min-h-screen bg-white">
      <Header />
      {/* Hero Section - Full Screen with Advanced Animations */}
      <section className="relative h-screen flex items-center justify-center overflow-hidden">
    {/* Animated Background - Теперь на всю секцию */}
    <div className="absolute inset-0 bg-gradient-to-br from-gray-50 via-blue-50 to-white">
          {/* Animated Gradient Orbs */}
          <motion.div
            animate={{
              scale: [1, 1.2, 1],
              opacity: [0.3, 0.5, 0.3],
              x: [0, 100, 0],
              y: [0, -100, 0],
            }}
            transition={{
              duration: 20,
              repeat: Infinity,
              ease: "easeInOut"
            }}
            className="absolute top-0 left-0 w-[500px] h-[500px] bg-[#2563EB] rounded-full blur-3xl opacity-20"
          />
          <motion.div
            animate={{
              scale: [1, 1.3, 1],
              opacity: [0.2, 0.4, 0.2],
              x: [0, -100, 0],
              y: [0, 100, 0],
            }}
            transition={{
              duration: 25,
              repeat: Infinity,
              ease: "easeInOut"
            }}
            className="absolute bottom-0 right-0 w-[600px] h-[600px] bg-[#1d4ed8] rounded-full blur-3xl opacity-20"
          />

          {/* Grid Pattern */}
          <div className="absolute inset-0 bg-[linear-gradient(to_right,#8080800a_1px,transparent_1px),linear-gradient(to_bottom,#8080800a_1px,transparent_1px)] bg-[size:14px_24px]"></div>
        </div>

        <div className="relative max-w-7xl mx-auto px-6 py-20">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <motion.div
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.8 }}
            >

              {/* Animated Headline */}
              <h1 className="text-6xl lg:text-7xl mb-6 leading-[1.1] ">
                <motion.span
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.6, delay: 0.2 }}
                  className="block"
                >
                  Управляйте
                </motion.span>
                <motion.span
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.6, delay: 0.4 }}
                  className="block"
                >
                  рекламой на{' '}
                   <span className="relative inline-block">
                        <span className="relative z-10 text-[#2563EB] font-semibold">всех экранах</span>
                        <motion.span
                        initial={{ scaleX: 0 }}
                        animate={{ scaleX: 1 }}
                        transition={{ duration: 0.8, delay: 0.8 }}
                        className=""
                        />
                    </span>
                </motion.span>
                <motion.span
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.6, delay: 0.6 }}
                  className="block bg-gradient-to-r from-[#2563EB] to-[#1d4ed8] bg-clip-text text-transparent font-semibold"
                >
                  из любой точки
                </motion.span>
              </h1>

              <motion.p
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 0.8, delay: 0.9 }}
                className="text-xl text-gray-600 mb-8 leading-relaxed"
              >
                Современная платформа для удаленного управления цифровыми экранами.
                Создавайте, планируйте и показывайте контент на тысячах устройств одновременно.
              </motion.p>

              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.8, delay: 1.1 }}
                className="flex flex-wrap gap-4"
              >
                <motion.div
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                >
                  <Button variant="primary" className="flex items-center gap-2 group"
                  onClick={() => token ? navigate("/crm") : navigate("/login") }>
                    Перейти в CRM
                    <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                  </Button>
                </motion.div>
                <motion.div
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                >
                  <Button variant="secondary" className="flex items-center gap-2"
                  onClick={() => {document.getElementById('demo-section')?.scrollIntoView({ behavior: 'smooth' })}}>
                    <Play className="w-5 h-5" />
                    Смотреть демо
                  </Button>
                </motion.div>
              </motion.div>

              {/* Stats */}
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 0.8, delay: 1.3 }}
                className="flex flex-wrap gap-8 mt-12"
              >
                <div>
                  <div className="text-3xl text-[#2563EB] mb-1">6</div>
                  <div className="text-sm text-gray-600">Поддерживаемых платформ</div>
                </div>
                <div>
                  <div className="text-3xl text-[#2563EB] mb-1">99.9%</div>
                  <div className="text-sm text-gray-600">Время работы</div>
                </div>
                <div>
                  <div className="text-3xl text-[#2563EB] mb-1">24/7</div>
                  <div className="text-sm text-gray-600">Поддержка</div>
                </div>
              </motion.div>
            </motion.div>

            <motion.div
            animate={{
                y: [0, -40, 0],
            }}
            transition={{
                duration: 6,
                repeat: Infinity,
                ease: "easeInOut"
            }}
            className="relative"
            >
            <ImageWithFallback
                src="./ad-screen.png"
                alt="Dashboard Interface"
                className="w-full h-full"
                style={{paddingLeft: "2rem"}}
            />
            </motion.div>
          </div>
        </div>


        {/* Scroll Indicator */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 1, delay: 1.5 }}
          className="absolute bottom-10 left-1/2 transform -translate-x-1/2"
        >
          <motion.div
            animate={{ y: [0, 10, 0] }}
            transition={{ duration: 2, repeat: Infinity }}
            className="flex flex-col items-center gap-2 text-gray-400"
          >
            <span className="text-sm">Прокрутите вниз</span>
            <div className="w-6 h-10 border-2 border-gray-300 rounded-full flex justify-center">
              <motion.div
                animate={{ y: [0, 12, 0] }}
                transition={{ duration: 2, repeat: Infinity }}
                className="w-1.5 h-1.5 bg-gray-400 rounded-full mt-2"
              />
            </div>
          </motion.div>
        </motion.div>
      </section>


      {/* How It Works Section */}
      <section className="py-20 bg-white">

        <div className="max-w-7xl mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
            className="text-center mb-16"
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.9 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5 }}
              className="inline-block"
            >
              <h2 className="text-5xl mb-4 bg-gradient-to-r from-gray-900 via-[#2563EB] to-gray-900 bg-clip-text text-transparent">
                Как это работает
              </h2>
            </motion.div>
            <p className="text-xl text-gray-600 max-w-2xl mx-auto">
              Четыре простых шага до запуска вашей рекламной кампании
            </p>
          </motion.div>

          <div className="max-w-3xl mx-auto space-y-8">
            {steps.map((step, index) => (
              <StepCard key={index} number={index + 1} {...step} index={index} />
            ))}
          </div>
        </div>
      </section>


<section
  id="demo-section"
  style={{
    padding: '80px 0',
    backgroundColor: '#f9fafb',
    minHeight: '100vh',
  }}
>
  <div
    style={{
      maxWidth: '1280px',
      margin: '0 auto',
      padding: '0 24px',
      textAlign: 'center',
    }}
  >
    <h2 className="text-5xl mb-4 bg-gradient-to-r from-gray-900 via-[#2563EB] to-gray-900 bg-clip-text text-transparent">
        Демо презентация проекта
    </h2>

    <p className="text-xl text-gray-600 max-w-2xl mx-auto mb-8">
        Современный дизайн и гибкость для любых сценариев
    </p>

    {/* Видео */}
    <div
      style={{
        maxWidth: '1024px',
        margin: '0 auto',
        borderRadius: '20px',
        overflow: 'hidden',
        boxShadow: '0 25px 50px rgba(0,0,0,0.15)',
        backgroundColor: '#000',
      }}
    >
      <iframe
        src="https://vk.com/video_ext.php?oid=-230317636&id=456239021&hash=2a9250445798b5dc&hd=1"
        width="1280"
        height="720"
        style={{
          display: 'block',
          width: '100%',
          height: 'calc(100vw * 9 / 16)',
          maxHeight: '576px',
          minHeight: '360px',
          border: 'none',
        }}
        allow="autoplay; encrypted-media; fullscreen; picture-in-picture;"
        allowFullScreen
      />
    </div>
  </div>
</section>

      {/* CTA Section */}
      <section className="py-20 relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-[#2563EB] to-[#1d4ed8]"></div>
        <div className="absolute inset-0 opacity-10">
          <div className="absolute top-0 left-0 w-96 h-96 bg-white rounded-full blur-3xl"></div>
          <div className="absolute bottom-0 right-0 w-96 h-96 bg-white rounded-full blur-3xl"></div>
        </div>

        <motion.div
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.7 }}
          className="relative max-w-4xl mx-auto px-6 text-center text-white"
        >
          <h2 className="text-4xl lg:text-5xl mb-6">
            Готовы начать?
          </h2>
          <Button variant="secondary" className="bg-white text-[#2563EB] hover:bg-gray-50 text-lg px-12 py-5"
          onClick={() => navigate("/login")}>
            Перейти в личный кабинет
          </Button>
        </motion.div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-gray-300 py-12">
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid md:grid-cols-4 gap-8 mb-8">
            <div>
              <div className="flex items-center gap-2 mb-4">
                <img alt="Логотип AdvertControl" src="/icon.ico" className="w-5 h-5 text-white" />
                <span className="text-white text-xl">AdvertControl</span>
              </div>
              <p className="text-sm text-gray-400">
                Современное решение для управления рекламными экранами
              </p>
            </div>

            <div>
              <h4 className="text-white mb-4">Продукт</h4>
              <ul className="space-y-2 text-sm">
                <li><a className="hover:text-white transition-colors">О сервисе</a></li>
                <li><a className="hover:text-white transition-colors">Тарифы</a></li>
                <li><a className="hover:text-white transition-colors">Функции</a></li>
                <li><a className="hover:text-white transition-colors">Обновления</a></li>
              </ul>
            </div>

            <div>
              <h4 className="text-white mb-4">Поддержка</h4>
              <ul className="space-y-2 text-sm">
                <li><a className="hover:text-white transition-colors">Документация</a></li>
                <li><a className="hover:text-white transition-colors">API</a></li>
                <li><a className="hover:text-white transition-colors">Помощь</a></li>
                <li><a className="hover:text-white transition-colors">Контакты</a></li>
              </ul>
            </div>

            <div>
              <h4 className="text-white mb-4">Контакты</h4>
              <ul className="space-y-2 text-sm">
                <li>support@advertcontrol.com</li>
                <li>Екатеринбург, Россия</li>
              </ul>
            </div>
          </div>

          <div className="pt-8 border-t border-gray-800 text-center text-sm text-gray-400">
            <p>© 2026 AdvertControl. СВАГА</p>
          </div>
        </div>
      </footer>
    </div>
  );
};
