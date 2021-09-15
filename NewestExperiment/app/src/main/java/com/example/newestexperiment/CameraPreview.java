package com.example.newestexperiment;

import android.content.Context;

import android.graphics.Matrix;
import android.hardware.Camera;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

import java.io.IOException;
import java.lang.reflect.Parameter;
import java.util.List;

public class CameraPreview extends SurfaceView implements SurfaceHolder.Callback {
    private static final String TAG = "surface err: ";
    private SurfaceHolder mHolder;
    private Camera mCamera;

    public CameraPreview(Context context, Camera camera) {
        super(context);
        mCamera = camera;

        // Install a SurfaceHolder.Callback so we get notified when the
        // underlying surface is created and destroyed.
        mHolder = getHolder();
        mHolder.addCallback(this);
    }

    public void surfaceCreated(SurfaceHolder holder) {
        // The Surface has been created, now tell the camera where to draw the preview.
        try {
            mCamera.setDisplayOrientation(90);
            Camera.Parameters parameters = mCamera.getParameters();
            parameters.setPreviewSize(getOptimalPreviewSize(mCamera.getParameters().getSupportedPreviewSizes(), 1800, 550).width,
                    getOptimalPreviewSize(mCamera.getParameters().getSupportedPreviewSizes(), 1800, 550).height);
//            Log.i("height", parameters.getPreviewSize().height+"");
//            for(Camera.Size i : mCamera.getParameters().getSupportedPreviewSizes()) {
//             //   Log.i("height: ", i.height + "");
//             //   Log.i("width: ", i.width + "");
//            }
//            Log.i("bbb", mCamera.getParameters().getSupportedPreviewSizes().toString());
            if (parameters.getSupportedFocusModes().contains(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE)) {
                parameters.setFocusMode(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE);
            } else {
                Log.e("nope", "no supported");
            }
            mCamera.setParameters(parameters);
            mCamera.setPreviewDisplay(holder);


            Matrix matrix = calculateSurfaceHolderTransform();
            float[] values = new float[9];

            matrix.getValues(values);
            this.setTranslationX(values[Matrix.MTRANS_X]);
            this.setTranslationY(values[Matrix.MTRANS_Y]);
            this.setScaleX(values[Matrix.MSCALE_X]);
            this.setScaleY(values[Matrix.MSCALE_Y]);
            this.invalidate();



            mCamera.startPreview();
        } catch (IOException e) {
            Log.d(TAG, "Error setting camera preview: " + e.getMessage());
        }
    }
    public static Camera.Size getOptimalPreviewSize(
            List<Camera.Size> sizes, int w, int h) {
        // Use a very small tolerance because we want an exact match.
        final double ASPECT_TOLERANCE = 0.1;
        double targetRatio = (double) h / w;
        if (sizes == null)
            return null;

        Camera.Size optimalSize = null;/*  w  w  w  .ja va  2 s .c  om*/

        // Start with max value and refine as we iterate over available preview sizes. This is the
        // minimum difference between view and camera height.
        double minDiff = Double.MAX_VALUE;

        // Target view height
        int targetHeight = h;

        // Try to find a preview size that matches aspect ratio and the target view size.
        // Iterate over all available sizes and pick the largest size that can fit in the view and
        // still maintain the aspect ratio.
        for (Camera.Size size : sizes) {
            double ratio = (double) size.width / size.height;
            if (Math.abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                continue;
            if (Math.abs(size.height - targetHeight) < minDiff) {
                optimalSize = size;
                minDiff = Math.abs(size.height - targetHeight);
            }
        }

        // Cannot find preview size that matches the aspect ratio, ignore the requirement
        if (optimalSize == null) {
            minDiff = Double.MAX_VALUE;
            for (Camera.Size size : sizes) {
                if (Math.abs(size.height - targetHeight) < minDiff) {
                    optimalSize = size;
                    minDiff = Math.abs(size.height - targetHeight);
                }
            }
        }

        return optimalSize;
    }

    public void surfaceDestroyed(SurfaceHolder holder) {

        // empty. Take care of releasing the Camera preview in your activity.
    }

    public void surfaceChanged(SurfaceHolder holder, int format, int w, int h) {
        // If your preview can change or rotate, take care of those events here.
        // Make sure to stop the preview before resizing or reformatting it.

        if (mHolder.getSurface() == null){
            // preview surface does not exist
            return;
        }

        // stop preview before making changes
        try {
            mCamera.stopPreview();
        } catch (Exception e){
            // ignore: tried to stop a non-existent preview
        }

        // set preview size and make any resize, rotate or
        // reformatting changes here

        // start preview with new settings
        try {
            mCamera.setPreviewDisplay(mHolder);
            mCamera.startPreview();

        } catch (Exception e){
            Log.d(TAG, "Error starting camera preview: " + e.getMessage());
        }
    }

    public Matrix calculateSurfaceHolderTransform() {
        int viewHeight = 550;
        int viewWidth = 1800;
        int cameraHeight = 600;

        int cameraWidth = 800;
        float ratioPreview = (float) cameraWidth / cameraHeight;
        float ratioView = (float) viewWidth / viewHeight;
        float scaleX, scaleY;
        if (ratioView < ratioPreview) {
            scaleX = ratioPreview / ratioView;
            scaleY = 1;
        } else {
            scaleX = 1;
            scaleY = ratioView / ratioPreview;
        }

// 计算出 View 的偏移量
        float scaledWidth = viewWidth * scaleX;
        float scaledHeight = viewHeight * scaleY;
        float dx = (viewWidth - scaledWidth) / 2;
        float dy = (viewHeight - scaledHeight) / 2;
        Matrix matrix = new Matrix();
        matrix.postScale(scaleX, scaleY);
        matrix.postTranslate(dx, dy);
        return matrix;

    }
}