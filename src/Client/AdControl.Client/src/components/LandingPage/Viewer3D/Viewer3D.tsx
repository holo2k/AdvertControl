import { Suspense } from 'react';
import { Canvas } from '@react-three/fiber';
import { Environment, ContactShadows, PresentationControls } from '@react-three/drei';
import { Model } from './Model';

interface Viewer3DProps {
    url: string;
    className?: string;
}

export default function Viewer3D({ url, className } : Viewer3DProps)  {
    return (
        <div className={className} style={{ width: '1200px', height: '700px', position: 'relative', marginTop: "70px"  }}>
            <Canvas
                shadows
                camera={{ position: [0, 0, 20], fov: 45 }}
                gl={{ antialias: true, alpha: true }}
            >
                <ambientLight intensity={0.7} />
                <pointLight position={[10, 10, 10]} intensity={1} />

                <Suspense fallback={null}>
                    <PresentationControls
                        global
                        cursor={true}
                        snap={1}
                        speed={2}
                        zoom={1}
                        polar={[0, 0]}
                        azimuth={[-Infinity, Infinity]}
                    >
                        <Model url={url} />
                    </PresentationControls>

                    <Environment preset="city" />
                </Suspense>
            </Canvas>
        </div>
    );
};
