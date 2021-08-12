/*

//https://it.wikipedia.org/wiki/Trasformata_di_Fourier_veloce



public class Complex
{
  double _r = 0;
  double _i = 0;
  const double M_PI = 3.14159265;

  public Complex()
  { }

  public Complex(Complex c)
  {
    _r = c._r;
    _i = c._i;
  }

  public Complex(double r, double i)
  {
    _r = r;
    _i = i;
  }

  public double Real { get { return _r; } set { _r = value; } }

  public  double Imag { get { return _i; } set { _i = value; }  }

  public void setPolar(double b, double phi)
  {
      _r = b * Math.Cos(phi);
      _i = b * Math.Sin(phi);
    }

  public double R { get { return Math.Sqrt(_r * _r + _i * _i); } }

  public double Phi {
    get {
      if (_r == 0) {
        if (_i > 0)
          return M_PI / 2;
        else
          return 1.5 * M_PI;

      } else {
        return Math.Atan(_i / _r);
      }
    }
  }

  static public Complex operator +(Complex c, Complex d)
  {
    Complex retVal = new Complex(d._r + c._r, d._i + c._i);
    return retVal;
  }

  static public Complex operator -(Complex c, Complex d)
  {
    Complex retVal = new Complex(d._r - c._r, d._i - c._i);
    return retVal;
  }

  static public Complex operator*(Complex c, Complex d)
  {
    Complex retVal = new Complex(
    d._r * c._r - d._i * c._i,
    d._r * c._i + d._i * c._r);
    return retVal;
  }

  static public Complex operator *(Complex c, double d)
  {
    Complex retVal = new Complex(
    c._r * d,
    c._i * d);
    return retVal;
  }

  /*static public void operator*=(Complex c, Complex d)
  {
    d._r *= c;
    d._i *= c;
  } */

  /*static public void operator=(double c) {
    _r = c;
    _i = 0;
  }*/
/*
  static Complex ePowI(double phi)
  {
    Complex retVal = new Complex(Math.Cos(phi), Math.Sin(phi));
    return retVal;
  }

  static public Complex pow(Complex c, double p) {
    Complex retVal = new Complex();
    retVal.setPolar(Math.Pow(c._r, p), p * c.Phi);
    return retVal;
  }
};





/ **
 * calculate FFT and IFFT (Supported FFT Lengths are 16, 64, 256, 1024) 
 * /
class Fft
{
  Complex[] _input;
  double m_maxValue;
  int m_maxIndex;
  float m_sampleRate;
  int _fftSize;

  public  Fft(int size)
  {
    _fftSize = size;
    _input = new Complex[size];
  }  

  / **
   * set a single value in the input buffer
   * /
  public void setInput(int i, float v) {
    Complex c = new Complex(v, 0);
    _input[i] = c;
  }

  / **
   * get amplitude at specified index
   * /
  public double getOutput(int i) {
     Complex c = new Complex(_input[i]);
     return Math.Sqrt(c.Real * c.Real + c.Imag * c.Imag); }
  
  / **
   * get frequency at specified index
   * /
  float getFrequency(int i)
  {
    return (float)i/ _fftSize * m_sampleRate; }

  / **
   * set sampling rate (for frequency calculation)
   * /
  void setSampleRate(float rate) { m_sampleRate = rate; }

  / **
   * do the calculation
   * /
  void process() {
     FFT(_input, _fftSize, 1.0);
  }
  

   int log2(int N)    //funzione per calcolare il logaritmo in base 2 di un intero
   {
    int k = N, i = 0;
    while(k > 0)
    {
       k >>= 1;
       i++;
     }
     return i - 1;
   }

   bool check(int n)    //usato per controllare se il numero di componenti del vettore di input è una potenza di 2
   {
     return (n > 0) && ((n & (n - 1)) == 0);
   }

   int reverse(int N, int n)    //calcola il reverse number di ogni intero n rispetto al numero massimo N
   {
     int j, p = 0;
    int xx = log2(N);
     for (j = 1; j <= xx; j++) 
    {
      if ((n & (1 << xx - j)) != 0)
         p |= 1 << (j - 1);
     }
     return p;
   }

   void ordina(Complex[] f1, int N)     //dispone gli elementi del vettore ordinandoli per reverse order
   {
     Complex[] f2 = new Complex[_fftSize];
     for(int i = 0; i < N; i++)
       f2[i] = f1[reverse(N, i)];
     for(int j = 0; j < N; j++)
       f1[j] = f2[j];
   }

   void transform(Complex[] f, int N)     //calcola il vettore trasformato
   {
     ordina(f, N);    //dapprima lo ordina col reverse order

     Complex[] W = new Complex[N / 2]; //vettore degli zeri dell'unità.
                               //Prima N/2-1 ma genera errore con ciclo for successivo
                              //in quanto prova a copiare in una zona non allocata "W[N/2-1]"
     W[1].setPolar(1.0, -2.0 * Math.PI / N);
     W[0] = new Complex(1,0);
     for(int i = 2; i < N / 2; i++)
       W[i] = Complex.pow(W[1], i);
     int n = 1;
     int a = N / 2;
     for(int j = 0; j < log2(N); j++) {
       for(int i = 0; i < N; i++) {
         if((i & n) != 0) {
           //ad ogni step di raddoppiamento di n, vengono utilizzati gli indici 
           //'i' presi alternativamente a gruppetti di n, una volta si e una no.
           Complex temp = new Complex( f[i]);
           Complex Temp =  new Complex(W[(i * a) % (n * a)] * f[i + n]);
           f[i] = temp + Temp;
           f[i + n] = temp - Temp;
         }
       }
       n *= 2;
       a = a / 2;
     }
   }

   void FFT(Complex[] f, int N, double d)
   {
     transform(f, N);
     for(int i = 0; i < N; i++)
       f[i] = f[i] * d; //moltiplica il vettore per il passo in modo da avere il vettore trasformato effettivo
   }
};
*/