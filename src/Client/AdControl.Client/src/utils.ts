export function getScreensWord(count: number): string {
  const lastDigit = count % 10;
  const lastTwoDigits = count % 100;

  // Исключения для чисел 11-14
  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    return `${count} экранов`;
  }

  switch (lastDigit) {
    case 1:
      return `${count} экран`;
    case 2:
    case 3:
    case 4:
      return `${count} экрана`;
    default:
      return `${count} экранов`;
  }
}