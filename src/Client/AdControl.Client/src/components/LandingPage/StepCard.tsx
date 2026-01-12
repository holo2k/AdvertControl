import { motion } from 'framer-motion';

interface StepCardProps {
  number: number;
  title: string;
  description: string;
  index: number;
}

export function StepCard({ number, title, description, index }: StepCardProps) {
  return (
    <motion.div
      initial={{ opacity: 0, x: -30 }}
      whileInView={{ opacity: 1, x: 0 }}
      viewport={{ once: true }}
      transition={{ duration: 0.5, delay: index * 0.15 }}
      className="flex gap-6 items-start"
    >
      <div className="flex-shrink-0 w-16 h-16 bg-gradient-to-br from-[#2563EB] to-[#1d4ed8] rounded-full flex items-center justify-center shadow-lg">
        <span className="text-white text-2xl">{number}</span>
      </div>
      <div className="flex-1">
        <h3 className="text-xl mb-2">{title}</h3>
        <p className="text-gray-600 leading-relaxed">{description}</p>
      </div>
    </motion.div>
  );
}
